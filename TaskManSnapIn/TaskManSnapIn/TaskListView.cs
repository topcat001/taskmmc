using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using System.Timers;

namespace Microsoft.ManagementConsole.TaskMan
{
    /// <summary>
    /// Displays the remote process list in the results pane.
    /// </summary>
    public class TaskListView : MmcListView
    {
        private System.Timers.Timer updateTimer;
        private delegate void DelegateWithView();
        private DelegateWithView delegateWithView = null;
        // Data gathered from the UI.
        private NetworkCredential credentials = null; // TODO Replace with a ConnectionManager object and factor out the WMI code there.
        private string serverName = null;
        private bool shouldUseRemote = false;
        private bool isUserRemote = false;
        private ulong updateInterval = 0;
        private ConnectionOptions connectionOptions = null;
        private string path = null;
        // Used for CPU% calculation.
        private Dictionary<uint, ulong> cpuTimesDict = null; // Kernel + user ticks.
        private Dictionary<uint, ulong> cpuTimesDictOld = null;
        private Dictionary<uint, ulong> wallTicksDict= null; // Wall clock ticks as opposed to kernel/user ticks.
        private Dictionary<uint, ulong> wallTicksDictOld = null;
        // Sort state.
        private int sortColumnIndex = -1;
        private bool sortDescending = false;
        // WMI thread.
        private Mutex wmiThreadMutex = new Mutex();
        private List<string[]> refreshData = new List<string[]>(); // Protected by wmiThreadMutex.

        /// <summary>
        /// Constructor.
        /// </summary>
        public TaskListView()
        {
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnInitialize(AsyncStatus status)
        {
            // First, initialise base.
            base.OnInitialize(status);

            // Gather data from the initialisation dialog.
            var snapIn = this.SnapIn as SnapIn;
            if (snapIn != null)
            {
                var iConnectionData = snapIn.RootNode as IConnectionData;
                if (iConnectionData != null)
                {
                    credentials = iConnectionData.ConnectionData.credentials;
                    serverName = iConnectionData.ConnectionData.serverName;
                    shouldUseRemote = iConnectionData.ConnectionData.shouldUseRemote;
                    isUserRemote = iConnectionData.ConnectionData.isUserRemote;
                    updateInterval = iConnectionData.ConnectionData.updateInterval;

                    if (shouldUseRemote && serverName != null)
                    {
                        connectionOptions = new ConnectionOptions
                        {
                            Impersonation = System.Management.ImpersonationLevel.Impersonate
                        };

                        if (isUserRemote)
                        {
                            connectionOptions.Username = credentials.UserName;
                            connectionOptions.Password = credentials.Password;
                        }

                        path = @"\\" + serverName + @"\root\cimv2";
                    }
                }
            }

            // Custom sorter since we want to treat the numerical columns as numbers and not string.
            Sorter = new TaskListSorter();

            // Default column. Always visible.
            Columns[0].Title = "Name";
            Columns[0].SetWidth(200);
            // The following columns are visible by default.
            Columns.Add(new MmcListViewColumn("PID", 50, MmcListViewColumnFormat.Right, true));                 // 1
            Columns.Add(new MmcListViewColumn("PPID", 50, MmcListViewColumnFormat.Right, true));                // 2
            Columns.Add(new MmcListViewColumn("CPU%", 50, MmcListViewColumnFormat.Right, true));                // 3
            Columns.Add(new MmcListViewColumn("Private Bytes (K)", 50, MmcListViewColumnFormat.Right, true));   // 4
            Columns.Add(new MmcListViewColumn("Working Set (K)", 50, MmcListViewColumnFormat.Right, true));     // 5
            // The following columns are hidden by default as they are slow to populate.
            Columns.Add(new MmcListViewColumn("Status", 100, MmcListViewColumnFormat.Left, false));             // 6
            Columns.Add(new MmcListViewColumn("User", 200, MmcListViewColumnFormat.Left, false));               // 7
            Columns.Add(new MmcListViewColumn("Commandline", 200, MmcListViewColumnFormat.Left, false));        // 8
            // TODO more columns.

            // Initialise list data.
            refreshData.Add(new string[] { "Gathering data...", "0", "0", "0", "0", "0", "-", "-", "-" }); // Dummy data for display at startup.
            // Populate the list.
            UpdateView();
            // Create a Refresh action that updates the list view.
            this.ActionsPaneItems.Add(new Action("Refresh", "refresh", -1, "Refresh"));
            // Set the function to be called by the WMI worker thread.
            delegateWithView = new DelegateWithView(UpdateView);

            // The snap-in is now initialised. Start an initial data gathering run via worker thread.
            Thread wmiThread = new Thread(WMIThreadProc);
            wmiThread.Start();

            // Finally, activate the timer.
            SetTimer();
        }

        /// <summary>
        /// Handle the execution of the global action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="status"></param>
        protected override void OnAction(Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "Refresh":
                    {
                        Refresh();
                        break;
                    }
            }
        }

        /// <summary>
        /// Implementation for the Refresh action.
        /// </summary>
        protected void Refresh()
        {
            // Bail if the worker thread is already running.
            if (!wmiThreadMutex.WaitOne(10))
                return;

            // We now hold the mutex, so release it first.
            wmiThreadMutex.ReleaseMutex();
            // Schedule a new worker thread.
            Thread wmiThread = new Thread(WMIThreadProc);
            wmiThread.Start();
        }

        /// <summary>
        /// Load the list with data.
        /// </summary>
        private void UpdateView()
        {
            ResultNodes.Clear();
            foreach(string[] row in refreshData)
            {
                ResultNode processNode = new ResultNode
                {
                    DisplayName = row[0] // Name - column 0
                };

                foreach (string entry in row.Skip(1))
                {
                    processNode.SubItemDisplayNames.Add(entry); // The rest of the columns for this row.
                }

                ResultNodes.Add(processNode);
            }

            // Sort if necessary.
            if (sortColumnIndex >= 0)
                Sort(sortColumnIndex, sortDescending);
        }

        /// <summary>
        /// Set up an update timer..
        /// </summary>
        private void SetTimer()
        {
            if (updateInterval > 0)
            {
                // Create a timer.
                updateTimer = new System.Timers.Timer(updateInterval);
                // Hookup the event for the timer
                updateTimer.Elapsed += OnTimedEvent;
                updateTimer.AutoReset = false;
                updateTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Timer callback.
        /// </summary>
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            // Bail if the worker thread is already running.
            if (wmiThreadMutex.WaitOne(10))
            {
                // We now hold the mutex, so release it first.
                wmiThreadMutex.ReleaseMutex();
                // Schedule a new worker thread.
                Thread wmiThread = new Thread(WMIThreadProc);
                wmiThread.Start();
                // Wait for it to finish.
                wmiThread.Join();
            }

            // Schedule the timer again.
            updateTimer.Start();
        }

        private void WMIThreadProc(object data)
        {
            // Protect access to refreshData.
            wmiThreadMutex.WaitOne();

            // Set up WMI query.
            ManagementObjectSearcher searcher = SearcherFromQueryString("SELECT * FROM Win32_Process");
            if (searcher != null)
            {
                refreshData.Clear();
                initPerRunCPUData();

                // This catches any exceptions caused by the Get() method. One cause is the remote auth failing.
                ManagementObjectCollection processManagementObjectCollection = null;
                try
                {
                    processManagementObjectCollection = searcher.Get();
                }
                catch (Exception)
                {
                    // Bail out.
                    wmiThreadMutex.ReleaseMutex();
                    return;
                }

                // The process objects in the following collection are "live". This means that the data retrieved
                // from a process object is not from time Get() was called but from the time the specific request
                // was made. This complicates the calculation of CPU utilisation. Also, we need to watch out for
                // processes which exit before we get to querying their properties (at which point the query
                // throws an exception).
                foreach (ManagementObject processManagementObject in processManagementObjectCollection)
                {
                    ROOT.CIMV2.Win32.Process process = new ROOT.CIMV2.Win32.Process(processManagementObject);
                    uint pid = process.ProcessId;
                    string owner = null;
                    if (Columns[7].Visible) // Domain\User - column 7
                    {
                        string domain = null;
                        string user = null;
                        process.GetOwner(out domain, out user); // This won't throw if the process is dead.
                        owner = domain + "\\" + user;
                    }
                    else
                        owner = "Disabled"; // This gives the sorter something if the column is hidden.

                    // Handle exceptions from dead processes.
                    try
                    {
                        refreshData.Add(new string[]
                        {
                            process.Caption, // Name - column 0
                            pid.ToString(), // PID - column 1
                            process.ParentProcessId.ToString(), // PPID - column 2
                            cpuPercent(process).ToString("N1"), // CPU% - column 3
                            (process.PrivatePageCount / 1024).ToString("N0"), // Private bytes - column 4
                            (process.WorkingSetSize / 1024).ToString("N0"), // Working set - column 5
                            Columns[6].Visible ? ProcessStatus(pid) : "Disabled", // Status - column 6
                            owner, // Domain\User - column 7
                            process.CommandLine ?? "" // Commandline - column 8
                        });
                    }
                    catch (Exception)
                    { }
                }

                // Update the display, and wait till complete.
                IAsyncResult result = SnapIn.BeginInvoke(delegateWithView, args: new object[] { });
                result.AsyncWaitHandle.WaitOne();
                // Not calling EndInvoke because we don't need the result.
            }

            // Release the mutex.
            wmiThreadMutex.ReleaseMutex();
        }

        /// <summary>
        /// Returns a ManagementObjectSearcher reference containing the query results.
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns><see cref="ManagementObjectSearcher"/></returns>
        private ManagementObjectSearcher SearcherFromQueryString(string queryString) // TODO This goes in the ConnectionManager.
        {
            if (shouldUseRemote && serverName != null)
            {
                // Process remote computer.
                if (connectionOptions == null)
                    return null; // Trouble!

                ManagementScope scope = new ManagementScope(
                    path,
                    connectionOptions);
                // This may throw an exception if the auth is unsuccessful.
                try
                {
                    scope.Connect();
                }
                catch (Exception)
                {
                    return null;
                }

                ObjectQuery query = new ObjectQuery(queryString);
                return new ManagementObjectSearcher(scope, query);
            }
            else
            {
                // Process local computer.
                return new ManagementObjectSearcher(
                    "root\\CIMV2",
                    queryString);
            }
        }

        /// <summary>
        /// Gets a process' status from its PID.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private string ProcessStatus(uint pid) // TODO This goes to ProcessHelper.
        {
            // Set up WMI query.
            ManagementObjectSearcher searcher = SearcherFromQueryString("Select * from Win32_Thread WHERE ProcessHandle = " + pid.ToString());
            if (searcher == null)
                return ""; // Trouble!

            bool areAllThreadsWaiting = true;
            foreach (ManagementObject threadManagementObject in searcher.Get())
            {
                ROOT.CIMV2.Win32.Thread thread = new ROOT.CIMV2.Win32.Thread(threadManagementObject);
                if (thread.ThreadWaitReason != ROOT.CIMV2.Win32.Thread.ThreadWaitReasonValues.FreePage0)
                {
                    areAllThreadsWaiting = false;
                    break;
                }
            }

            return areAllThreadsWaiting ? "Suspended" : "Running";
        }

        private void initPerRunCPUData() // TODO This goes to ProcessHelper.
        {
            // Manage CPU statistics.
            cpuTimesDictOld = cpuTimesDict;
            cpuTimesDict = new Dictionary<uint, ulong>();
            wallTicksDictOld = wallTicksDict;
            wallTicksDict = new Dictionary<uint, ulong>();
        }

        /// <summary>
        /// Calculates an approximation to the CPU utilisation between two successive runs.
        /// Note that the very first run will produce 0.0. Occasional spurious results are
        /// possible when PIDs are recycled.
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private double cpuPercent(ROOT.CIMV2.Win32.Process process) // TODO This goes to ProcessHelper.
        {
            // Calculate the user + kernel time for the process.
            var pid = process.ProcessId;
            var newProcessTicks = process.UserModeTime + process.KernelModeTime;
            cpuTimesDict[pid] = newProcessTicks;
            var newWallTicks = (ulong)DateTime.Now.Ticks;
            wallTicksDict[pid] = newWallTicks;

            if (wallTicksDictOld == null || cpuTimesDictOld == null)
                return 0.0;

            try
            {
                var oldProcessTicks = cpuTimesDictOld[pid];
                var elapsedProcessTicks = newProcessTicks - oldProcessTicks;
                if (elapsedProcessTicks <= 0)
                    return 0.0;

                var oldWallTicks = wallTicksDictOld[pid];
                var elapsedWallTicks = newWallTicks - oldWallTicks;
                if (elapsedWallTicks <= 0)
                    return 0.0;

                return ((double)elapsedProcessTicks / (double)elapsedWallTicks * 100.0);
            }
            catch (KeyNotFoundException)
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Saves the sort settings currently in use.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="descending"></param>
        protected override void OnSortCompleted(int columnIndex, bool descending)
        {
            base.OnSortCompleted(columnIndex, descending);
            sortColumnIndex = columnIndex;
            sortDescending = descending;
        }

        /// <summary>
        /// Handles changes in list view selection. Only acts on the first selected row.
        /// </summary>
        /// <param name="status"></param>
        protected override void OnSelectionChanged(SyncStatus status)
        {
            int count = SelectedNodes.Count;

            // Update selection context.
            if (count == 0)
            {
                SelectionData.Clear();
                SelectionData.ActionsPaneItems.Clear();
            }
            else
            {
                // Update the console with the selection information.
                SelectionData.Update((ResultNode)SelectedNodes[0], count > 1, null, null);
                SelectionData.ActionsPaneItems.Clear();
                SelectionData.ActionsPaneItems.Add(new Action("Terminate", "Terminate process", -1, "Terminate"));
            }
        }

        /// <summary>
        /// Handle the action for selected result node.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="status"></param>
        protected override void OnSelectionAction(Action action, AsyncStatus status)
        {
            switch ((string)action.Tag)
            {
                case "Terminate":
                    {
                        // Extract the pid of the underlying process.
                        var selectedNode = (ResultNode)SelectionData.SelectionObject;
                        var pidString = selectedNode.SubItemDisplayNames[0];
                        // Set up WMI query.
                        ManagementObjectSearcher searcher = SearcherFromQueryString("SELECT * FROM Win32_Process WHERE ProcessID = " + pidString);
                        if (searcher == null)
                            break; // Trouble!

                        foreach (ManagementObject processManagementObject in searcher.Get())
                        {
                            ROOT.CIMV2.Win32.Process process = new ROOT.CIMV2.Win32.Process(processManagementObject);
                            process.Terminate(0);
                        }

                        break;
                    }
            }
        }
    } // class
} // namespace
using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.ManagementConsole.TaskMan
{
    /// <summary>
    /// RunInstaller attribute - Allows the .Net framework InstallUtil.exe to install the assembly.
    /// SnapInInstaller class - Installs snap-in for MMC.
    /// </summary>
    [RunInstaller(true)]
    public class InstallUtilSupport : SnapInInstaller
    {
    }

    /// <summary>
    /// Connection data for remote access.
    /// </summary>
    public struct ConnectionData
    {
        public NetworkCredential credentials;
        public string serverName;
        public bool shouldUseRemote;
        public bool isUserRemote;
        public ulong updateInterval;
    }

    /// <summary>
    /// Interface exposing <see cref="ConnectionData"/>.
    /// </summary>
    public interface IConnectionData
    {
        ConnectionData ConnectionData { get; set; }
    }

    /// <summary>
    /// Scope node implementation for the task manager.
    /// </summary>
    public class TaskManScopeNode : ScopeNode, IConnectionData
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TaskManScopeNode()
        {
        }

        public ConnectionData ConnectionData { get; set; }

        // TODO: Property page for this node.
    } // class

    /// <summary>
    /// SnapInSettings attribute - Used to set the registration information for the snap-in.
    /// SnapIn class - Provides the main entry point for the creation of a snap-in. 
    /// TaskManSnapIn class - Implements the remote task manager snap-in.  
    /// </summary>
    [SnapInSettings("{ef169c9e-fd6d-4b00-aed9-3b28a5842cf6}",
         DisplayName = "TaskMan",
         Description = "Remote Task Manager SnapIn",
         Vendor = "Quintessential Trinkets")]
    [SnapInAbout("TaskManRes.dll",
        ApplicationBaseRelative = true,
        VersionId = 101,
        IconId = 102,
        LargeFolderBitmapId = 104,
        SmallFolderBitmapId = 103,
        SmallFolderSelectedBitmapId = 103)]
    public class TaskManSnapIn : SnapIn
    {
        /// <summary>
        /// Constructor
        /// </summary>
		public TaskManSnapIn()
        {
            // Add generic icons here.
            // Application specific icons are managed elsewhere (TODO).
            //SmallImages.Add(new Icon(SystemIcons.Application, 16, 16));
            string iconRes = AppDomain.CurrentDomain.BaseDirectory + "\\TaskManRes.dll";
            SmallImages.Add(UnmanagedMethods.ExtractIcon(iconRes, 0, false));
            //LargeImages.Add(new Icon(SystemIcons.Application, 32, 32));
            LargeImages.Add(UnmanagedMethods.ExtractIcon(iconRes, 0, true));

            // Create root node in the left pane.
            RootNode = new TaskManScopeNode
            {
                DisplayName = "TaskMan",
                ImageIndex = 0,
                SelectedImageIndex = 0
                //EnabledStandardVerbs = StandardVerbs.Properties
                // TODO: Is it useful to implement copy?
            };

            // When true, mmc will save data from the snap-in.
            IsModified = true;

            // We have two views: A list view and a tree view.
            // The list view.
            MmcListViewDescription listViewDescription = new MmcListViewDescription
            {
                DisplayName = "List view",
                ViewType = typeof(TaskListView),
                Options = 
                MmcListViewOptions.ExcludeScopeNodes |
                MmcListViewOptions.SingleSelect |
                MmcListViewOptions.UseCustomSorting
            };

            // The tree view.
            // TODO

            // Attach views to the root node.
            RootNode.ViewDescriptions.Add(listViewDescription);
            RootNode.ViewDescriptions.DefaultIndex = 0;
        }

        /// <summary>
        /// Adds the computer name to the node display name.
        /// </summary>
        /// <param name="iConnectionData"></param>
        private void SetDisplayName(IConnectionData iConnectionData)
        {
            if (iConnectionData.ConnectionData.shouldUseRemote)
            {
                if (iConnectionData.ConnectionData.serverName != null)
                {
                    RootNode.DisplayName += " (" + iConnectionData.ConnectionData.serverName + ")";
                }
            }
            else
            {
                RootNode.DisplayName += " (local)";
            }
        }

        /// <summary>
        /// Show the initialization wizard when the snap-in is added to console.
        /// </summary>
        /// <returns></returns>
        protected override bool OnShowInitializationWizard()
        {
            // Show a modal dialog to get the snap-in name.
            InitialisationWizard initialisationWizard = new InitialisationWizard();
            // The return value of ShowDialog() is basically the value of the Form.DialogResult
            // property inherited by InitialisationWizard.
            bool result = (Console.ShowDialog(initialisationWizard) == DialogResult.OK);

            if (result)
            {
                var iConnectionData = RootNode as IConnectionData;
                if (iConnectionData != null)
                {
                    iConnectionData.ConnectionData = new ConnectionData
                    {
                        credentials = initialisationWizard.NetworkCredentials,
                        serverName = initialisationWizard.ServerName,
                        shouldUseRemote = initialisationWizard.ShouldUseRemote,
                        isUserRemote = initialisationWizard.IsUserRemote,
                        updateInterval = initialisationWizard.UpdateInterval * 1000 // Convert from secs to ms.
                    };

                    // Set the name of the computer being monitored.
                    SetDisplayName(iConnectionData);
                }
            }

            return result;
        }

        /// <summary>
        /// Load any unsaved data.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="persistenceData"></param>
        protected override void OnLoadCustomData(AsyncStatus status, byte[] persistenceData)
        {
            // Restore snapin state from saved data.
            string data = Encoding.Unicode.GetString(persistenceData);
            if (!string.IsNullOrEmpty(data))
            {
                string[] splitData = data.Split(new string[] { "__" }, StringSplitOptions.None);
                // Data format is "shouldUseRemote__serverName__updateInterval".
                var iConnectionData = RootNode as IConnectionData;
                iConnectionData.ConnectionData = new ConnectionData
                {
                    credentials = new NetworkCredential(),
                    serverName = splitData[1],
                    shouldUseRemote = Convert.ToBoolean(splitData[0]),
                    isUserRemote = false,
                    updateInterval = Convert.ToUInt32(splitData[2])
                };

                // Set the name of the computer being monitored.
                SetDisplayName(iConnectionData);
            }
        }

        /// <summary>
        /// If the snap-in has been modified, then save the data.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected override byte[] OnSaveCustomData(SyncStatus status)
        {
            // Save the connection data as a byte stream.
            var iConnectionData = RootNode as IConnectionData;
            if (iConnectionData != null)
            {
                // We do not save network credentials.
                // Data format is "shouldUseRemote__serverName__updateInterval".
                string saveData = iConnectionData.ConnectionData.shouldUseRemote.ToString() + "__";
                string serverName = iConnectionData.ConnectionData.serverName;
                saveData += (serverName == null) ? "__" : serverName + "__";
                saveData += iConnectionData.ConnectionData.updateInterval.ToString();
                return Encoding.Unicode.GetBytes(saveData);
            }
            else
            {
                // This should not happen.
                throw new System.Exception("Connection data missing during exit!");
            }
        }
    } // class
} // namespace
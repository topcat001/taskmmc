using System;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.ManagementConsole.TaskMan
{
    /// <summary>
    /// Implements a browser for computers on a network.
    /// Based off https://stackoverflow.com/questions/6930827/need-a-dialog-box-to-browse-computers-on-a-network
    /// </summary>
    public class ComputerBrowser
    {
        private FolderBrowserFolder _startLocation = FolderBrowserFolder.NetworkNeighborhood;
        private BrowseInfoFlags _options = BrowseInfoFlags.BrowseForComputer | BrowseInfoFlags.NewDialogStyle;
        private static readonly int MAX_PATH;
        private string _title;
        private string _displayName;
        private string _path;

        /// <summary>
        /// static constructor.
        /// </summary>
        static ComputerBrowser()
        {
            MAX_PATH = 260;
        }

        /// <summary>
        /// Displays the computer browser dialog.
        /// </summary>
        public bool ShowDialog()
        {
            return ShowDialog(null);
        }

        /// <summary>
        /// Displays the computer browser dialog.
        /// </summary>
        ///  <param name="owner">Parent window handle</param>
        public bool ShowDialog(IWin32Window owner)
        {
            // Setup for calling SHGetSpecialFolderLocation.
            _path = string.Empty;
            IntPtr handle;
            IntPtr pidlRoot = IntPtr.Zero;
            if (owner != null)
                handle = owner.Handle;
            else
                handle = UnmanagedMethods.GetActiveWindow();

            UnmanagedMethods.SHGetSpecialFolderLocation(handle, (int)_startLocation, ref pidlRoot);
            if (pidlRoot == IntPtr.Zero)
                return false;

            if ((_options & BrowseInfoFlags.NewDialogStyle) != 0)
                Application.OleRequired();
            IntPtr pidl = IntPtr.Zero;
            try
            {
                // Setup for calling SHBrowseForFolder.
                BrowseInfo lpbi = new BrowseInfo
                {
                    pidlRoot = pidlRoot,
                    hwndOwner = handle,
                    displayName = new string('\0', MAX_PATH),
                    title = _title,
                    flags = (int)_options,
                    callback = null,
                    lparam = IntPtr.Zero
                };

                pidl = UnmanagedMethods.SHBrowseForFolder(ref lpbi);
                if (pidl == IntPtr.Zero)
                    return false;
                _displayName = lpbi.displayName;

                // Convert returned path id list to a string.
                StringBuilder pathReturned = new StringBuilder(MAX_PATH);
                UnmanagedMethods.SHGetPathFromIDList(pidl, pathReturned);
                _path = pathReturned.ToString();

                // Free memory for path id list.
                UnmanagedMethods.SHMemFree(pidl);
            }
            finally
            {
                // Free memory for the root path id list.
                UnmanagedMethods.SHMemFree(pidlRoot);
            }

            return true;
        }

        /// <summary>
        /// Browse for computers on the network and return a string name.
        /// </summary>
        ///  <param name="title">Title for the browser dialog</param>
        public static string GetComputerName(string title)
        {
            ComputerBrowser browser = new ComputerBrowser
            {
                _title = title
            };
            if (browser.ShowDialog())
                return browser._displayName;
            else
                return string.Empty;
        }
    } // class
} // namespace
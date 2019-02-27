using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.ManagementConsole.TaskMan
{
    /// <summary>
    /// Create the form that gets the name of the snap-in.
    /// </summary>
    public partial class InitialisationWizard : Form
    {
        private NetworkCredential credentials;

        /// <summary>
        /// Gets the credentials entered by the user. Read only.
        /// </summary>
        public NetworkCredential NetworkCredentials
        {
            get
            {
                return credentials;
            }
        }

        /// <summary>
        /// Gets the remote computer name. Read only.
        /// </summary>
        public string ServerName
        {
            get
            {
                return compNameTextBox.Text;
            }
        }

        /// <summary>
        /// Gets the state of the remote computer radio button. Read only.
        /// </summary>
        public bool ShouldUseRemote
        {
            get
            {
                return remoteCompRadioButton.Checked;
            }
        }

        /// <summary>
        /// Gets the state of pick user check box. Read only.
        /// </summary>
        public bool IsUserRemote
        {
            get
            {
                return userCheckBox.Checked;
            }
        }

        public ulong UpdateInterval
        {
            get
            {
                return (ulong)refreshNumeric.Value;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public InitialisationWizard()
        {
            InitializeComponent();
            // Final state setup.
            ActivateRemoteCompInputs();
        }

        /// <summary>
        /// Handles a click of the Ok button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ok_Click(object sender, EventArgs e) => DialogResult = DialogResult.OK;

        /// <summary>
        /// Handles a click of the Browse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompBrowser_Click(object sender, EventArgs e) => compNameTextBox.Text = ComputerBrowser.GetComputerName("Select Computer");

        /// <summary>
        /// Handles state changes of the Remote computer radio button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoteCompRadioButton_CheckedChanged(object sender, EventArgs e) => ActivateRemoteCompInputs();

        /// <summary>
        /// Handles state changes of the Pick remote user check box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoteUserCheckBox_CheckedChanged(object sender, EventArgs e) => ActivateRemoteCompInputs();

        /// <summary>
        /// Enables or disables the remote computer name entry controls based on the state
        /// of the <see cref="remoteCompRadioButton"/> member.
        /// </summary>
        private void ActivateRemoteCompInputs()
        {
            compNameTextBox.Enabled = remoteCompRadioButton.Checked;
            compBrowserButton.Enabled = remoteCompRadioButton.Checked;
            userCheckBox.Enabled = remoteCompRadioButton.Checked;
            userLabel.Enabled = remoteCompRadioButton.Checked && userCheckBox.Checked;
            userButton.Enabled = remoteCompRadioButton.Checked && userCheckBox.Checked;
        }

        private void GetCredentials(string serverName, out NetworkCredential networkCredential)
        {
            CREDUI_INFO credui = new CREDUI_INFO
            {
                pszCaptionText = "Please enter the credentails for " + serverName,
                pszMessageText = "Remote User"
            };
            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            IntPtr outCredBuffer = new IntPtr();
            uint outCredSize;
            bool save = false;
            uint result = UnmanagedMethods.CredUIPromptForWindowsCredentials(
                ref credui,
                0,
                ref authPackage,
                IntPtr.Zero,
                0,
                out outCredBuffer,
                out outCredSize,
                ref save,
                PromptForWindowsCredentialsFlags.CREDUIWIN_GENERIC /* Generic */);

            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf = new StringBuilder(100);

            int maxUserName = 100;
            int maxDomain = 100;
            int maxPassword = 100;
            if (result == 0)
            {
                if (UnmanagedMethods.CredUnPackAuthenticationBuffer(
                    0,
                    outCredBuffer, outCredSize,
                    usernameBuf, ref maxUserName,
                    domainBuf, ref maxDomain,
                    passwordBuf, ref maxPassword))
                {
                    //clear the memory allocated by CredUIPromptForWindowsCredentials
                    UnmanagedMethods.CoTaskMemFree(outCredBuffer);
                    networkCredential = new NetworkCredential()
                    {
                        UserName = usernameBuf.ToString(),
                        Password = passwordBuf.ToString(),
                        Domain = domainBuf.ToString()
                    };

                    // Update the display.
                    userLabel.Text = networkCredential.UserName;
                    return;
                }
            }

            networkCredential = null;
        }

        /// <summary>
        /// Handles a click of the Set user button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetUser_Click(object sender, EventArgs e) => GetCredentials(compNameTextBox.Text, out credentials);
    } // class
} // namespace
using System.Windows.Forms;

namespace Microsoft.ManagementConsole.TaskMan
{
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    partial class InitialisationWizard
    {
        private Label promptLabel;
        private RadioButton localCompRadioButton;
        private TextBox compNameTextBox;
        private Button compBrowserButton;
        private RadioButton remoteCompRadioButton;
        private Button okButton;
        private Button cancelButton;
        private CheckBox userCheckBox;
        private Label userLabel;
        private Button userButton;
        private Label refreshLabel;
        private NumericUpDown refreshNumeric;
        private Label separatorLabel;

        private void InitializeComponent()
        {
            this.promptLabel = new System.Windows.Forms.Label();
            this.localCompRadioButton = new System.Windows.Forms.RadioButton();
            this.remoteCompRadioButton = new System.Windows.Forms.RadioButton();
            this.compNameTextBox = new System.Windows.Forms.TextBox();
            this.compBrowserButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.userCheckBox = new System.Windows.Forms.CheckBox();
            this.userLabel = new System.Windows.Forms.Label();
            this.userButton = new System.Windows.Forms.Button();
            this.refreshLabel = new System.Windows.Forms.Label();
            this.refreshNumeric = new System.Windows.Forms.NumericUpDown();
            this.separatorLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.refreshNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Location = new System.Drawing.Point(24, 40);
            this.promptLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(503, 25);
            this.promptLabel.TabIndex = 0;
            this.promptLabel.Text = "Select the computer you want to view processes on";
            // 
            // localCompRadioButton
            // 
            this.localCompRadioButton.AutoSize = true;
            this.localCompRadioButton.Location = new System.Drawing.Point(30, 96);
            this.localCompRadioButton.Margin = new System.Windows.Forms.Padding(6);
            this.localCompRadioButton.Name = "localCompRadioButton";
            this.localCompRadioButton.Size = new System.Drawing.Size(590, 29);
            this.localCompRadioButton.TabIndex = 1;
            this.localCompRadioButton.TabStop = true;
            this.localCompRadioButton.Text = "Local Computer (the computer this console is running on)";
            this.localCompRadioButton.UseVisualStyleBackColor = true;
            // 
            // remoteCompRadioButton
            // 
            this.remoteCompRadioButton.AutoSize = true;
            this.remoteCompRadioButton.Location = new System.Drawing.Point(30, 140);
            this.remoteCompRadioButton.Margin = new System.Windows.Forms.Padding(6);
            this.remoteCompRadioButton.Name = "remoteCompRadioButton";
            this.remoteCompRadioButton.Size = new System.Drawing.Size(219, 29);
            this.remoteCompRadioButton.TabIndex = 2;
            this.remoteCompRadioButton.TabStop = true;
            this.remoteCompRadioButton.Text = "Another computer:";
            this.remoteCompRadioButton.UseVisualStyleBackColor = true;
            this.remoteCompRadioButton.CheckedChanged += new System.EventHandler(this.RemoteCompRadioButton_CheckedChanged);
            // 
            // compNameTextBox
            // 
            this.compNameTextBox.Location = new System.Drawing.Point(326, 140);
            this.compNameTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.compNameTextBox.Name = "compNameTextBox";
            this.compNameTextBox.Size = new System.Drawing.Size(650, 31);
            this.compNameTextBox.TabIndex = 3;
            // 
            // compBrowserButton
            // 
            this.compBrowserButton.Location = new System.Drawing.Point(992, 133);
            this.compBrowserButton.Margin = new System.Windows.Forms.Padding(6);
            this.compBrowserButton.Name = "compBrowserButton";
            this.compBrowserButton.Size = new System.Drawing.Size(166, 48);
            this.compBrowserButton.TabIndex = 4;
            this.compBrowserButton.Text = "Browse...";
            this.compBrowserButton.UseVisualStyleBackColor = true;
            this.compBrowserButton.Click += new System.EventHandler(this.CompBrowser_Click);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(841, 318);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(135, 59);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.Ok_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(1019, 318);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(135, 59);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // userCheckBox
            // 
            this.userCheckBox.AutoSize = true;
            this.userCheckBox.Location = new System.Drawing.Point(29, 188);
            this.userCheckBox.Name = "userCheckBox";
            this.userCheckBox.Size = new System.Drawing.Size(211, 29);
            this.userCheckBox.TabIndex = 7;
            this.userCheckBox.Text = "Pick remote user:";
            this.userCheckBox.UseVisualStyleBackColor = true;
            this.userCheckBox.CheckedChanged += new System.EventHandler(this.RemoteUserCheckBox_CheckedChanged);
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Location = new System.Drawing.Point(246, 192);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(101, 25);
            this.userLabel.TabIndex = 8;
            this.userLabel.Text = "<default>";
            // 
            // userButton
            // 
            this.userButton.Location = new System.Drawing.Point(992, 203);
            this.userButton.Name = "userButton";
            this.userButton.Size = new System.Drawing.Size(166, 48);
            this.userButton.TabIndex = 9;
            this.userButton.Text = "Set user...";
            this.userButton.UseVisualStyleBackColor = true;
            this.userButton.Click += new System.EventHandler(this.SetUser_Click);
            // 
            // refreshLabel
            // 
            this.refreshLabel.AutoSize = true;
            this.refreshLabel.Location = new System.Drawing.Point(29, 233);
            this.refreshLabel.Name = "refreshLabel";
            this.refreshLabel.Size = new System.Drawing.Size(158, 25);
            this.refreshLabel.TabIndex = 10;
            this.refreshLabel.Text = "Refresh (secs):";
            // 
            // refreshNumeric
            // 
            this.refreshNumeric.Location = new System.Drawing.Point(251, 231);
            this.refreshNumeric.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.refreshNumeric.Name = "refreshNumeric";
            this.refreshNumeric.Size = new System.Drawing.Size(120, 31);
            this.refreshNumeric.TabIndex = 11;
            // 
            // separatorLabel
            // 
            this.separatorLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separatorLabel.Location = new System.Drawing.Point(30, 280);
            this.separatorLabel.Name = "separatorLabel";
            this.separatorLabel.Size = new System.Drawing.Size(1124, 10);
            this.separatorLabel.TabIndex = 12;
            // 
            // InitialisationWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 408);
            this.Controls.Add(this.separatorLabel);
            this.Controls.Add(this.refreshNumeric);
            this.Controls.Add(this.refreshLabel);
            this.Controls.Add(this.userButton);
            this.Controls.Add(this.userLabel);
            this.Controls.Add(this.userCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.compBrowserButton);
            this.Controls.Add(this.compNameTextBox);
            this.Controls.Add(this.remoteCompRadioButton);
            this.Controls.Add(this.localCompRadioButton);
            this.Controls.Add(this.promptLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "InitialisationWizard";
            this.Text = "TaskMan - Select Computer";
            ((System.ComponentModel.ISupportInitialize)(this.refreshNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    } // class
} // namespace
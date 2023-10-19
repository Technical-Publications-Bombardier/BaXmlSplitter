namespace BaXmlSplitter
{
    partial class SettingsPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsPage));
            settingsGroup = new GroupBox();
            arrowsLabel = new Label();
            StorageAccountKeyTwoLabel = new Label();
            AzureStorageKeyTwoTextBox = new TextBox();
            StorageAccountKeyOneLabel = new Label();
            AzureStorageKeyOneTextBox = new TextBox();
            BaOraConnectionStringLabel = new Label();
            BaOraConnectionStringTextBox = new TextBox();
            AzureApplicationSecretLabel = new Label();
            AzureApplicationSecretTextBox = new TextBox();
            HashiCorpClientSecretTextBox = new TextBox();
            hashiCorpClientSecretLabel = new Label();
            cancelSettings = new Button();
            okSettings = new Button();
            applySettings = new Button();
            splitContainer1 = new SplitContainer();
            settingsGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // settingsGroup
            // 
            settingsGroup.BackgroundImage = Properties.Resources.arrows;
            settingsGroup.Controls.Add(arrowsLabel);
            settingsGroup.Controls.Add(StorageAccountKeyTwoLabel);
            settingsGroup.Controls.Add(AzureStorageKeyTwoTextBox);
            settingsGroup.Controls.Add(StorageAccountKeyOneLabel);
            settingsGroup.Controls.Add(AzureStorageKeyOneTextBox);
            settingsGroup.Controls.Add(BaOraConnectionStringLabel);
            settingsGroup.Controls.Add(BaOraConnectionStringTextBox);
            settingsGroup.Controls.Add(AzureApplicationSecretLabel);
            settingsGroup.Controls.Add(AzureApplicationSecretTextBox);
            settingsGroup.Controls.Add(HashiCorpClientSecretTextBox);
            settingsGroup.Controls.Add(hashiCorpClientSecretLabel);
            settingsGroup.Location = new Point(3, 3);
            settingsGroup.Name = "settingsGroup";
            settingsGroup.Size = new Size(575, 261);
            settingsGroup.TabIndex = 0;
            settingsGroup.TabStop = false;
            settingsGroup.Text = "Secrets Retrieval";
            // 
            // arrowsLabel
            // 
            arrowsLabel.AutoSize = true;
            arrowsLabel.Location = new Point(416, 25);
            arrowsLabel.Name = "arrowsLabel";
            arrowsLabel.Size = new Size(144, 180);
            arrowsLabel.TabIndex = 13;
            arrowsLabel.Text = resources.GetString("arrowsLabel.Text");
            // 
            // StorageAccountKeyTwoLabel
            // 
            StorageAccountKeyTwoLabel.AutoSize = true;
            StorageAccountKeyTwoLabel.Location = new Point(6, 224);
            StorageAccountKeyTwoLabel.Name = "StorageAccountKeyTwoLabel";
            StorageAccountKeyTwoLabel.Size = new Size(174, 15);
            StorageAccountKeyTwoLabel.TabIndex = 12;
            StorageAccountKeyTwoLabel.Text = "Azure Storage Account Key Two";
            StorageAccountKeyTwoLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // AzureStorageKeyTwoTextBox
            // 
            AzureStorageKeyTwoTextBox.Location = new Point(6, 198);
            AzureStorageKeyTwoTextBox.Name = "AzureStorageKeyTwoTextBox";
            AzureStorageKeyTwoTextBox.Size = new Size(270, 23);
            AzureStorageKeyTwoTextBox.TabIndex = 11;
            // 
            // StorageAccountKeyOneLabel
            // 
            StorageAccountKeyOneLabel.AutoSize = true;
            StorageAccountKeyOneLabel.Location = new Point(0, 180);
            StorageAccountKeyOneLabel.Name = "StorageAccountKeyOneLabel";
            StorageAccountKeyOneLabel.Size = new Size(175, 15);
            StorageAccountKeyOneLabel.TabIndex = 10;
            StorageAccountKeyOneLabel.Text = "Azure Storage Account Key One";
            StorageAccountKeyOneLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // AzureStorageKeyOneTextBox
            // 
            AzureStorageKeyOneTextBox.Location = new Point(6, 154);
            AzureStorageKeyOneTextBox.Name = "AzureStorageKeyOneTextBox";
            AzureStorageKeyOneTextBox.Size = new Size(270, 23);
            AzureStorageKeyOneTextBox.TabIndex = 9;
            // 
            // BaOraConnectionStringLabel
            // 
            BaOraConnectionStringLabel.AutoSize = true;
            BaOraConnectionStringLabel.Location = new Point(6, 136);
            BaOraConnectionStringLabel.Name = "BaOraConnectionStringLabel";
            BaOraConnectionStringLabel.Size = new Size(205, 15);
            BaOraConnectionStringLabel.TabIndex = 8;
            BaOraConnectionStringLabel.Text = "Bombardier Oracle Connection String";
            BaOraConnectionStringLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // BaOraConnectionStringTextBox
            // 
            BaOraConnectionStringTextBox.Location = new Point(6, 110);
            BaOraConnectionStringTextBox.Name = "BaOraConnectionStringTextBox";
            BaOraConnectionStringTextBox.Size = new Size(270, 23);
            BaOraConnectionStringTextBox.TabIndex = 7;
            // 
            // AzureApplicationSecretLabel
            // 
            AzureApplicationSecretLabel.AutoSize = true;
            AzureApplicationSecretLabel.Location = new Point(0, 92);
            AzureApplicationSecretLabel.Name = "AzureApplicationSecretLabel";
            AzureApplicationSecretLabel.Size = new Size(136, 15);
            AzureApplicationSecretLabel.TabIndex = 6;
            AzureApplicationSecretLabel.Text = "Azure Application Secret";
            AzureApplicationSecretLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // AzureApplicationSecretTextBox
            // 
            AzureApplicationSecretTextBox.Location = new Point(6, 66);
            AzureApplicationSecretTextBox.Name = "AzureApplicationSecretTextBox";
            AzureApplicationSecretTextBox.Size = new Size(270, 23);
            AzureApplicationSecretTextBox.TabIndex = 5;
            // 
            // HashiCorpClientSecretTextBox
            // 
            HashiCorpClientSecretTextBox.Location = new Point(6, 22);
            HashiCorpClientSecretTextBox.Name = "HashiCorpClientSecretTextBox";
            HashiCorpClientSecretTextBox.Size = new Size(270, 23);
            HashiCorpClientSecretTextBox.TabIndex = 4;
            HashiCorpClientSecretTextBox.UseSystemPasswordChar = true;
            // 
            // hashiCorpClientSecretLabel
            // 
            hashiCorpClientSecretLabel.AutoSize = true;
            hashiCorpClientSecretLabel.Location = new Point(6, 48);
            hashiCorpClientSecretLabel.Name = "hashiCorpClientSecretLabel";
            hashiCorpClientSecretLabel.Size = new Size(132, 15);
            hashiCorpClientSecretLabel.TabIndex = 0;
            hashiCorpClientSecretLabel.Text = "HashiCorp Client Secret";
            hashiCorpClientSecretLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // cancelSettings
            // 
            cancelSettings.Location = new Point(419, 400);
            cancelSettings.Name = "cancelSettings";
            cancelSettings.Size = new Size(75, 23);
            cancelSettings.TabIndex = 3;
            cancelSettings.Text = "Cancel";
            cancelSettings.UseVisualStyleBackColor = true;
            cancelSettings.Click += CancelSettings_Click;
            // 
            // okSettings
            // 
            okSettings.Location = new Point(338, 400);
            okSettings.Name = "okSettings";
            okSettings.Size = new Size(75, 23);
            okSettings.TabIndex = 2;
            okSettings.Text = "OK";
            okSettings.UseVisualStyleBackColor = true;
            okSettings.Click += OkSettings_Click;
            // 
            // applySettings
            // 
            applySettings.Location = new Point(500, 400);
            applySettings.Name = "applySettings";
            applySettings.Size = new Size(75, 23);
            applySettings.TabIndex = 1;
            applySettings.Text = "Apply";
            applySettings.UseVisualStyleBackColor = true;
            applySettings.Click += ApplySettings_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Location = new Point(12, 12);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(settingsGroup);
            splitContainer1.Panel2.Controls.Add(applySettings);
            splitContainer1.Panel2.Controls.Add(okSettings);
            splitContainer1.Panel2.Controls.Add(cancelSettings);
            splitContainer1.Size = new Size(776, 426);
            splitContainer1.SplitterDistance = 194;
            splitContainer1.TabIndex = 1;
            // 
            // SettingsPage
            // 
            AcceptButton = okSettings;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelSettings;
            ClientSize = new Size(800, 450);
            Controls.Add(splitContainer1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SettingsPage";
            Text = "Xml Splitter Settings";
            settingsGroup.ResumeLayout(false);
            settingsGroup.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox settingsGroup;
        private Button cancelSettings;
        private Button okSettings;
        private Button applySettings;
        private Label hashiCorpClientSecretLabel;
        private TextBox HashiCorpClientSecretTextBox;
        private GlyphableCheckbox showClientSecret = new(Properties.Resources.EyeOpen, Properties.Resources.EyeClosed);
        private Label AzureApplicationSecretLabel;
        private TextBox AzureApplicationSecretTextBox;
        private Label StorageAccountKeyTwoLabel;
        private TextBox AzureStorageKeyTwoTextBox;
        private Label StorageAccountKeyOneLabel;
        private TextBox AzureStorageKeyOneTextBox;
        private Label BaOraConnectionStringLabel;
        private TextBox BaOraConnectionStringTextBox;
        private SplitContainer splitContainer1;
        private Label arrowsLabel;
    }
}
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
            hcpClient.Dispose();
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
            secretsGroup = new GroupBox();
            showAzureStorageAccountKeyTwoCheckbox = new ShowPasswordCheckbox();
            showAzureStorageAccountKeyOneCheckbox = new ShowPasswordCheckbox();
            showBaOraConnectionStringCheckbox = new ShowPasswordCheckbox();
            showAzureApplicationSecretCheckbox = new ShowPasswordCheckbox();
            showHashiCorpClientSecretCheckbox = new ShowPasswordCheckbox();
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
            splitSettings = new SplitContainer();
            languageGroup = new GroupBox();
            languageComboBox = new ComboBox();
            secretsGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitSettings).BeginInit();
            splitSettings.Panel1.SuspendLayout();
            splitSettings.Panel2.SuspendLayout();
            splitSettings.SuspendLayout();
            languageGroup.SuspendLayout();
            SuspendLayout();
            // 
            // secretsGroup
            // 
            secretsGroup.BackgroundImage = Properties.Resources.arrows;
            secretsGroup.Controls.Add(showAzureStorageAccountKeyTwoCheckbox);
            secretsGroup.Controls.Add(showAzureStorageAccountKeyOneCheckbox);
            secretsGroup.Controls.Add(showBaOraConnectionStringCheckbox);
            secretsGroup.Controls.Add(showAzureApplicationSecretCheckbox);
            secretsGroup.Controls.Add(showHashiCorpClientSecretCheckbox);
            secretsGroup.Controls.Add(arrowsLabel);
            secretsGroup.Controls.Add(StorageAccountKeyTwoLabel);
            secretsGroup.Controls.Add(AzureStorageKeyTwoTextBox);
            secretsGroup.Controls.Add(StorageAccountKeyOneLabel);
            secretsGroup.Controls.Add(AzureStorageKeyOneTextBox);
            secretsGroup.Controls.Add(BaOraConnectionStringLabel);
            secretsGroup.Controls.Add(BaOraConnectionStringTextBox);
            secretsGroup.Controls.Add(AzureApplicationSecretLabel);
            secretsGroup.Controls.Add(AzureApplicationSecretTextBox);
            secretsGroup.Controls.Add(HashiCorpClientSecretTextBox);
            secretsGroup.Controls.Add(hashiCorpClientSecretLabel);
            secretsGroup.Location = new Point(3, 3);
            secretsGroup.Name = "secretsGroup";
            secretsGroup.Size = new Size(575, 261);
            secretsGroup.TabIndex = 0;
            secretsGroup.TabStop = false;
            secretsGroup.Text = "Secrets Retrieval";
            // 
            // showAzureStorageAccountKeyTwoCheckbox
            // 
            showAzureStorageAccountKeyTwoCheckbox.AutoSize = true;
            showAzureStorageAccountKeyTwoCheckbox.CheckmarkGlyph = (Image)resources.GetObject("showAzureStorageAccountKeyTwoCheckbox.CheckmarkGlyph");
            showAzureStorageAccountKeyTwoCheckbox.Location = new Point(258, 202);
            showAzureStorageAccountKeyTwoCheckbox.Name = "showAzureStorageAccountKeyTwoCheckbox";
            showAzureStorageAccountKeyTwoCheckbox.Size = new Size(15, 14);
            showAzureStorageAccountKeyTwoCheckbox.TabIndex = 18;
            showAzureStorageAccountKeyTwoCheckbox.UnCheckmarkGlyph = (Image)resources.GetObject("showAzureStorageAccountKeyTwoCheckbox.UnCheckmarkGlyph");
            showAzureStorageAccountKeyTwoCheckbox.CheckedChanged += ShowAzureStorageAccountKeyTwo_Click;
            // 
            // showAzureStorageAccountKeyOneCheckbox
            // 
            showAzureStorageAccountKeyOneCheckbox.AutoSize = true;
            showAzureStorageAccountKeyOneCheckbox.CheckmarkGlyph = (Image)resources.GetObject("showAzureStorageAccountKeyOneCheckbox.CheckmarkGlyph");
            showAzureStorageAccountKeyOneCheckbox.Location = new Point(259, 158);
            showAzureStorageAccountKeyOneCheckbox.Name = "showAzureStorageAccountKeyOneCheckbox";
            showAzureStorageAccountKeyOneCheckbox.Size = new Size(15, 14);
            showAzureStorageAccountKeyOneCheckbox.TabIndex = 17;
            showAzureStorageAccountKeyOneCheckbox.UnCheckmarkGlyph = (Image)resources.GetObject("showAzureStorageAccountKeyOneCheckbox.UnCheckmarkGlyph");
            showAzureStorageAccountKeyOneCheckbox.CheckedChanged += ShowAzureStorageAccountKeyOne_Click;
            // 
            // showBaOraConnectionStringCheckbox
            // 
            showBaOraConnectionStringCheckbox.AutoSize = true;
            showBaOraConnectionStringCheckbox.CheckmarkGlyph = (Image)resources.GetObject("showBaOraConnectionStringCheckbox.CheckmarkGlyph");
            showBaOraConnectionStringCheckbox.Location = new Point(258, 115);
            showBaOraConnectionStringCheckbox.Name = "showBaOraConnectionStringCheckbox";
            showBaOraConnectionStringCheckbox.Size = new Size(15, 14);
            showBaOraConnectionStringCheckbox.TabIndex = 16;
            showBaOraConnectionStringCheckbox.UnCheckmarkGlyph = (Image)resources.GetObject("showBaOraConnectionStringCheckbox.UnCheckmarkGlyph");
            showBaOraConnectionStringCheckbox.CheckedChanged += ShowBaOraConnectionString_Click;
            // 
            // showAzureApplicationSecretCheckbox
            // 
            showAzureApplicationSecretCheckbox.AutoSize = true;
            showAzureApplicationSecretCheckbox.CheckmarkGlyph = (Image)resources.GetObject("showAzureApplicationSecretCheckbox.CheckmarkGlyph");
            showAzureApplicationSecretCheckbox.Location = new Point(259, 70);
            showAzureApplicationSecretCheckbox.Name = "showAzureApplicationSecretCheckbox";
            showAzureApplicationSecretCheckbox.Size = new Size(15, 14);
            showAzureApplicationSecretCheckbox.TabIndex = 15;
            showAzureApplicationSecretCheckbox.UnCheckmarkGlyph = (Image)resources.GetObject("showAzureApplicationSecretCheckbox.UnCheckmarkGlyph");
            showAzureApplicationSecretCheckbox.CheckedChanged += ShowAzureApplicationSecret_Click;
            // 
            // showHashiCorpClientSecretCheckbox
            // 
            showHashiCorpClientSecretCheckbox.AutoSize = true;
            showHashiCorpClientSecretCheckbox.CheckmarkGlyph = (Image)resources.GetObject("showHashiCorpClientSecretCheckbox.CheckmarkGlyph");
            showHashiCorpClientSecretCheckbox.Location = new Point(258, 27);
            showHashiCorpClientSecretCheckbox.Name = "showHashiCorpClientSecretCheckbox";
            showHashiCorpClientSecretCheckbox.Size = new Size(15, 14);
            showHashiCorpClientSecretCheckbox.TabIndex = 14;
            showHashiCorpClientSecretCheckbox.UnCheckmarkGlyph = (Image)resources.GetObject("showHashiCorpClientSecretCheckbox.UnCheckmarkGlyph");
            showHashiCorpClientSecretCheckbox.CheckedChanged += ShowHashiCorpClientSecret_Click;
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
            AzureStorageKeyTwoTextBox.UseSystemPasswordChar = true;
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
            AzureStorageKeyOneTextBox.UseSystemPasswordChar = true;
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
            BaOraConnectionStringTextBox.UseSystemPasswordChar = true;
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
            AzureApplicationSecretTextBox.UseSystemPasswordChar = true;
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
            // splitSettings
            // 
            splitSettings.Location = new Point(12, 12);
            splitSettings.Name = "splitSettings";
            // 
            // splitSettings.Panel1
            // 
            splitSettings.Panel1.Controls.Add(languageGroup);
            // 
            // splitSettings.Panel2
            // 
            splitSettings.Panel2.Controls.Add(secretsGroup);
            splitSettings.Panel2.Controls.Add(applySettings);
            splitSettings.Panel2.Controls.Add(okSettings);
            splitSettings.Panel2.Controls.Add(cancelSettings);
            splitSettings.Size = new Size(776, 426);
            splitSettings.SplitterDistance = 194;
            splitSettings.TabIndex = 1;
            // 
            // languageGroup
            // 
            languageGroup.Controls.Add(languageComboBox);
            languageGroup.Location = new Point(1, 0);
            languageGroup.Name = "languageGroup";
            languageGroup.Size = new Size(194, 426);
            languageGroup.TabIndex = 0;
            languageGroup.TabStop = false;
            languageGroup.Text = "Language";
            // 
            // languageComboBox
            // 
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Location = new Point(6, 21);
            languageComboBox.Name = "languageComboBox";
            languageComboBox.Size = new Size(182, 23);
            languageComboBox.TabIndex = 0;
            // 
            // SettingsPage
            // 
            AcceptButton = okSettings;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelSettings;
            ClientSize = new Size(800, 450);
            Controls.Add(splitSettings);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SettingsPage";
            Text = "Xml Splitter Settings";
            Load += LoadSecrets;
            secretsGroup.ResumeLayout(false);
            secretsGroup.PerformLayout();
            splitSettings.Panel1.ResumeLayout(false);
            splitSettings.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitSettings).EndInit();
            splitSettings.ResumeLayout(false);
            languageGroup.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

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
        private SplitContainer splitSettings;
        private Label arrowsLabel;
        private GroupBox secretsGroup;
        private ShowPasswordCheckbox showAzureApplicationSecretCheckbox;
        private ShowPasswordCheckbox showHashiCorpClientSecretCheckbox;
        private ShowPasswordCheckbox showAzureStorageAccountKeyTwoCheckbox;
        private ShowPasswordCheckbox showAzureStorageAccountKeyOneCheckbox;
        private ShowPasswordCheckbox showBaOraConnectionStringCheckbox;
        private GroupBox languageGroup;
        private ComboBox languageComboBox;
    }
}
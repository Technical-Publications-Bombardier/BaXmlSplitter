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
            hashiCorpClientSecretTextBox = new TextBox();
            cancelSettings = new Button();
            okSettings = new Button();
            applySettings = new Button();
            hashiCorpClientSecretLabel = new Label();
            settingsGroup.SuspendLayout();
            SuspendLayout();
            // 
            // settingsGroup
            // 
            settingsGroup.Controls.Add(showClientSecret);
            settingsGroup.Controls.Add(hashiCorpClientSecretTextBox);
            settingsGroup.Controls.Add(cancelSettings);
            settingsGroup.Controls.Add(okSettings);
            settingsGroup.Controls.Add(applySettings);
            settingsGroup.Controls.Add(hashiCorpClientSecretLabel);
            settingsGroup.Location = new Point(12, 12);
            settingsGroup.Name = "settingsGroup";
            settingsGroup.Size = new Size(776, 426);
            settingsGroup.TabIndex = 0;
            settingsGroup.TabStop = false;
            settingsGroup.Text = "Secrets Retrieval";
            // 
            // showClientSecret
            // 
            showClientSecret.CheckmarkGlyph = Properties.Resources.EyeOpen;
            showClientSecret.Location = new Point(738, 26);
            showClientSecret.Name = "showClientSecret";
            showClientSecret.Size = new Size(20, 16);
            showClientSecret.TabIndex = 5;
            showClientSecret.UnCheckmarkGlyph = Properties.Resources.EyeClosed;
            showClientSecret.UseVisualStyleBackColor = true;
            showClientSecret.Click += ShowHashiCorpClientSecret_Click;
            // 
            // hashiCorpClientSecretTextBox
            // 
            hashiCorpClientSecretTextBox.Location = new Point(6, 22);
            hashiCorpClientSecretTextBox.Name = "hashiCorpClientSecretTextBox";
            hashiCorpClientSecretTextBox.Size = new Size(764, 23);
            hashiCorpClientSecretTextBox.TabIndex = 4;
            hashiCorpClientSecretTextBox.UseSystemPasswordChar = true;
            // 
            // cancelSettings
            // 
            cancelSettings.Location = new Point(614, 397);
            cancelSettings.Name = "cancelSettings";
            cancelSettings.Size = new Size(75, 23);
            cancelSettings.TabIndex = 3;
            cancelSettings.Text = "Cancel";
            cancelSettings.UseVisualStyleBackColor = true;
            cancelSettings.Click += CancelSettings_Click;
            // 
            // okSettings
            // 
            okSettings.Location = new Point(533, 397);
            okSettings.Name = "okSettings";
            okSettings.Size = new Size(75, 23);
            okSettings.TabIndex = 2;
            okSettings.Text = "OK";
            okSettings.UseVisualStyleBackColor = true;
            okSettings.Click += OkSettings_Click;
            // 
            // applySettings
            // 
            applySettings.Location = new Point(695, 397);
            applySettings.Name = "applySettings";
            applySettings.Size = new Size(75, 23);
            applySettings.TabIndex = 1;
            applySettings.Text = "Apply";
            applySettings.UseVisualStyleBackColor = true;
            applySettings.Click += ApplySettings_Click;
            // 
            // hashiCorpClientSecretLabel
            // 
            hashiCorpClientSecretLabel.AutoSize = true;
            hashiCorpClientSecretLabel.Location = new Point(638, 48);
            hashiCorpClientSecretLabel.Name = "hashiCorpClientSecretLabel";
            hashiCorpClientSecretLabel.Size = new Size(132, 15);
            hashiCorpClientSecretLabel.TabIndex = 0;
            hashiCorpClientSecretLabel.Text = "HashiCorp Client Secret";
            hashiCorpClientSecretLabel.TextAlign = ContentAlignment.TopRight;
            // 
            // SettingsPage
            // 
            AcceptButton = okSettings;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelSettings;
            ClientSize = new Size(800, 450);
            Controls.Add(settingsGroup);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SettingsPage";
            Text = "Xml Splitter Settings";
            settingsGroup.ResumeLayout(false);
            settingsGroup.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox settingsGroup;
        private Button cancelSettings;
        private Button okSettings;
        private Button applySettings;
        private Label hashiCorpClientSecretLabel;
        private TextBox hashiCorpClientSecretTextBox;
        private GlyphableCheckbox showClientSecret = new(Properties.Resources.EyeOpen, Properties.Resources.EyeClosed);
    }
}
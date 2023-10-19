using BaXmlSplitter.Properties;
using System.Drawing.Text;

namespace BaXmlSplitter
{
    partial class XmlSplitter
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XmlSplitter));
            xmlSelectBox = new GroupBox();
            xmlSelectTextBox = new TextBox();
            xmlSelectLabel = new Label();
            xmlButton = new Button();
            uowSelectBox = new GroupBox();
            uowTextBox = new TextBox();
            uowLabel = new Label();
            uowButton = new Button();
            stepsPanel = new Panel();
            programGroupBox = new GroupBox();
            programLabel = new Label();
            programsComboBox = new ComboBox();
            execButton = new Button();
            outputBox = new GroupBox();
            outDirWillBeCreated = new Label();
            outDirTextBox = new TextBox();
            dirOutLabel = new Label();
            outDirButton = new Button();
            xpathGroupBox = new GroupBox();
            xpathTextBox = new TextBox();
            xpathLabel = new Label();
            logGroupBox = new GroupBox();
            logTextBox = new RichTextBox();
            xmlSelectBox.SuspendLayout();
            uowSelectBox.SuspendLayout();
            stepsPanel.SuspendLayout();
            programGroupBox.SuspendLayout();
            outputBox.SuspendLayout();
            xpathGroupBox.SuspendLayout();
            logGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // xmlSelectBox
            // 
            xmlSelectBox.Controls.Add(xmlSelectTextBox);
            xmlSelectBox.Controls.Add(xmlSelectLabel);
            xmlSelectBox.Controls.Add(xmlButton);
            xmlSelectBox.Font = new Font("72", 11.25F, FontStyle.Bold);
            xmlSelectBox.Location = new Point(0, 3);
            xmlSelectBox.Name = "xmlSelectBox";
            xmlSelectBox.Size = new Size(489, 100);
            xmlSelectBox.TabIndex = 0;
            xmlSelectBox.TabStop = false;
            xmlSelectBox.Text = "Step 1";
            // 
            // xmlSelectTextBox
            // 
            xmlSelectTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            xmlSelectTextBox.AutoCompleteSource = AutoCompleteSource.FileSystem;
            xmlSelectTextBox.Location = new Point(6, 41);
            xmlSelectTextBox.Name = "xmlSelectTextBox";
            xmlSelectTextBox.Size = new Size(402, 25);
            xmlSelectTextBox.TabIndex = 0;
            xmlSelectTextBox.TextChanged += XmlSelectTextBox_TextChanged;
            // 
            // xmlSelectLabel
            // 
            xmlSelectLabel.AutoSize = true;
            xmlSelectLabel.Font = new Font("72", 11.25F);
            xmlSelectLabel.Location = new Point(6, 21);
            xmlSelectLabel.Name = "xmlSelectLabel";
            xmlSelectLabel.Size = new Size(200, 17);
            xmlSelectLabel.TabIndex = 0;
            xmlSelectLabel.Text = "Select manual in XML to split";
            // 
            // xmlButton
            // 
            xmlButton.Location = new Point(414, 40);
            xmlButton.Name = "xmlButton";
            xmlButton.Size = new Size(75, 26);
            xmlButton.TabIndex = 1;
            xmlButton.Text = "Browse";
            xmlButton.UseVisualStyleBackColor = true;
            xmlButton.Click += BrowseXml;
            // 
            // uowSelectBox
            // 
            uowSelectBox.Controls.Add(uowTextBox);
            uowSelectBox.Controls.Add(uowLabel);
            uowSelectBox.Controls.Add(uowButton);
            uowSelectBox.Font = new Font("72", 11.25F, FontStyle.Bold);
            uowSelectBox.Location = new Point(0, 109);
            uowSelectBox.Name = "uowSelectBox";
            uowSelectBox.Size = new Size(489, 100);
            uowSelectBox.TabIndex = 2;
            uowSelectBox.TabStop = false;
            uowSelectBox.Text = "Step 2";
            // 
            // uowTextBox
            // 
            uowTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            uowTextBox.AutoCompleteSource = AutoCompleteSource.FileSystem;
            uowTextBox.Location = new Point(6, 41);
            uowTextBox.Name = "uowTextBox";
            uowTextBox.Size = new Size(402, 25);
            uowTextBox.TabIndex = 2;
            uowTextBox.TextChanged += UowStatesTextBox_TextChanged;
            // 
            // uowLabel
            // 
            uowLabel.AutoSize = true;
            uowLabel.Font = new Font("72", 11.25F);
            uowLabel.Location = new Point(6, 21);
            uowLabel.Name = "uowLabel";
            uowLabel.Size = new Size(206, 17);
            uowLabel.TabIndex = 2;
            uowLabel.Text = "Select units of work states file";
            // 
            // uowButton
            // 
            uowButton.Location = new Point(414, 40);
            uowButton.Name = "uowButton";
            uowButton.Size = new Size(75, 26);
            uowButton.TabIndex = 3;
            uowButton.Text = "Browse";
            uowButton.UseVisualStyleBackColor = true;
            uowButton.Click += BrowseUow;
            // 
            // stepsPanel
            // 
            stepsPanel.Controls.Add(programGroupBox);
            stepsPanel.Controls.Add(execButton);
            stepsPanel.Controls.Add(outputBox);
            stepsPanel.Controls.Add(uowSelectBox);
            stepsPanel.Controls.Add(xmlSelectBox);
            stepsPanel.Location = new Point(12, 12);
            stepsPanel.Name = "stepsPanel";
            stepsPanel.Size = new Size(489, 448);
            stepsPanel.TabIndex = 0;
            // 
            // programGroupBox
            // 
            programGroupBox.Controls.Add(programLabel);
            programGroupBox.Controls.Add(programsComboBox);
            programGroupBox.Font = new Font("72", 11.25F, FontStyle.Bold);
            programGroupBox.Location = new Point(0, 321);
            programGroupBox.Name = "programGroupBox";
            programGroupBox.Size = new Size(489, 83);
            programGroupBox.TabIndex = 6;
            programGroupBox.TabStop = false;
            programGroupBox.Text = "Step 4";
            programGroupBox.Enter += ProgramGroupBox;
            // 
            // programLabel
            // 
            programLabel.AutoSize = true;
            programLabel.Font = new Font("72", 11.25F);
            programLabel.Location = new Point(6, 21);
            programLabel.Name = "programLabel";
            programLabel.Size = new Size(110, 17);
            programLabel.TabIndex = 4;
            programLabel.Text = "Select program";
            // 
            // programsComboBox
            // 
            programsComboBox.AutoCompleteCustomSource.AddRange(new string[] { "B_IFM", "CH604PROD", "CTALPROD", "LJ4045PROD", "GXPROD" });
            programsComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            programsComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            programsComboBox.FormattingEnabled = true;
            programsComboBox.Items.AddRange(new object[] { "B_IFM", "CH604PROD", "CTALPROD", "GXPROD", "LJ4045PROD" });
            programsComboBox.Location = new Point(6, 41);
            programsComboBox.MaxLength = 10;
            programsComboBox.Name = "programsComboBox";
            programsComboBox.Size = new Size(284, 25);
            programsComboBox.Sorted = true;
            programsComboBox.TabIndex = 3;
            programsComboBox.DropDown += ProgramGroupBox;
            programsComboBox.SelectionChangeCommitted += ProgramGroupBox;
            programsComboBox.TextUpdate += ProgramGroupBox;
            programsComboBox.DropDownClosed += ProgramGroupBox;
            programsComboBox.TextChanged += ProgramGroupBox;
            programsComboBox.Leave += ProgramGroupBox;
            programsComboBox.Validated += ProgramGroupBox;
            // 
            // execButton
            // 
            execButton.Dock = DockStyle.Bottom;
            execButton.Font = new Font("72", 11.25F, FontStyle.Bold);
            execButton.Location = new Point(0, 416);
            execButton.Name = "execButton";
            execButton.Size = new Size(489, 32);
            execButton.TabIndex = 5;
            execButton.Text = "Execute Splitting";
            execButton.UseVisualStyleBackColor = true;
            execButton.Click += ExecuteSplit;
            // 
            // outputBox
            // 
            outputBox.Controls.Add(outDirWillBeCreated);
            outputBox.Controls.Add(outDirTextBox);
            outputBox.Controls.Add(dirOutLabel);
            outputBox.Controls.Add(outDirButton);
            outputBox.Font = new Font("72", 11.25F, FontStyle.Bold);
            outputBox.Location = new Point(0, 215);
            outputBox.Name = "outputBox";
            outputBox.Size = new Size(489, 100);
            outputBox.TabIndex = 4;
            outputBox.TabStop = false;
            outputBox.Text = "Step 3";
            // 
            // outDirWillBeCreated
            // 
            outDirWillBeCreated.AccessibleDescription = "When visible, this indicates that the output directory will be created";
            outDirWillBeCreated.AccessibleName = "Will be created";
            outDirWillBeCreated.AccessibleRole = AccessibleRole.StaticText;
            outDirWillBeCreated.AutoSize = true;
            outDirWillBeCreated.Font = new Font("72", 11.25F, FontStyle.Bold | FontStyle.Italic);
            outDirWillBeCreated.ForeColor = Color.ForestGreen;
            outDirWillBeCreated.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Polite;
            outDirWillBeCreated.Location = new Point(6, 69);
            outDirWillBeCreated.Name = "outDirWillBeCreated";
            outDirWillBeCreated.Size = new Size(111, 17);
            outDirWillBeCreated.TabIndex = 3;
            outDirWillBeCreated.Text = "Will be created";
            outDirWillBeCreated.Visible = false;
            // 
            // outDirTextBox
            // 
            outDirTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            outDirTextBox.AutoCompleteSource = AutoCompleteSource.FileSystem;
            outDirTextBox.Location = new Point(6, 41);
            outDirTextBox.Name = "outDirTextBox";
            outDirTextBox.Size = new Size(402, 25);
            outDirTextBox.TabIndex = 4;
            outDirTextBox.TextChanged += OutDirTextBox_TextChanged;
            // 
            // dirOutLabel
            // 
            dirOutLabel.AutoSize = true;
            dirOutLabel.Font = new Font("72", 11.25F);
            dirOutLabel.Location = new Point(6, 21);
            dirOutLabel.Name = "dirOutLabel";
            dirOutLabel.Size = new Size(160, 17);
            dirOutLabel.TabIndex = 4;
            dirOutLabel.Text = "Select output directory";
            // 
            // outDirButton
            // 
            outDirButton.Location = new Point(414, 40);
            outDirButton.Name = "outDirButton";
            outDirButton.Size = new Size(75, 26);
            outDirButton.TabIndex = 5;
            outDirButton.Text = "Browse";
            outDirButton.UseVisualStyleBackColor = true;
            outDirButton.Click += BrowseOutDir;
            // 
            // xpathGroupBox
            // 
            xpathGroupBox.Controls.Add(xpathTextBox);
            xpathGroupBox.Controls.Add(xpathLabel);
            xpathGroupBox.Font = new Font("72", 11.25F, FontStyle.Bold);
            xpathGroupBox.Location = new Point(506, 12);
            xpathGroupBox.Name = "xpathGroupBox";
            xpathGroupBox.Size = new Size(552, 209);
            xpathGroupBox.TabIndex = 1;
            xpathGroupBox.TabStop = false;
            xpathGroupBox.Text = "Effective XPath";
            xpathGroupBox.Visible = false;
            // 
            // xpathTextBox
            // 
            xpathTextBox.AccessibleDescription = "The XPath query that would select the nodes on which the XML will be split";
            xpathTextBox.AccessibleName = "XPath";
            xpathTextBox.AccessibleRole = AccessibleRole.Text;
            xpathTextBox.Location = new Point(12, 45);
            xpathTextBox.Multiline = true;
            xpathTextBox.Name = "xpathTextBox";
            xpathTextBox.ReadOnly = true;
            xpathTextBox.ScrollBars = ScrollBars.Vertical;
            xpathTextBox.Size = new Size(534, 158);
            xpathTextBox.TabIndex = 1;
            xpathTextBox.TextChanged += XPathTextBox_TextChanged;
            // 
            // xpathLabel
            // 
            xpathLabel.AutoSize = true;
            xpathLabel.Font = new Font("72", 11.25F);
            xpathLabel.Location = new Point(6, 21);
            xpathLabel.Name = "xpathLabel";
            xpathLabel.Size = new Size(505, 17);
            xpathLabel.TabIndex = 0;
            xpathLabel.Text = "The XPath query that would select the nodes on which the XML will be split";
            // 
            // logGroupBox
            // 
            logGroupBox.Controls.Add(logTextBox);
            logGroupBox.Font = new Font("72", 11.25F, FontStyle.Bold);
            logGroupBox.Location = new Point(518, 227);
            logGroupBox.Name = "logGroupBox";
            logGroupBox.Size = new Size(534, 230);
            logGroupBox.TabIndex = 2;
            logGroupBox.TabStop = false;
            logGroupBox.Text = "Output Log";
            // 
            // logTextBox
            // 
            logTextBox.AccessibleDescription = "The log of output from the program";
            logTextBox.AccessibleName = "Output log";
            logTextBox.AccessibleRole = AccessibleRole.Text;
            logTextBox.BackColor = SystemColors.Desktop;
            logTextBox.Font = new Font("72 Monospace", 11.25F);
            logTextBox.ForeColor = Color.Lime;
            logTextBox.Location = new Point(-6, 24);
            logTextBox.Name = "logTextBox";
            logTextBox.ReadOnly = true;
            logTextBox.RightToLeft = RightToLeft.No;
            logTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            logTextBox.Size = new Size(540, 198);
            logTextBox.TabIndex = 0;
            logTextBox.Text = "";
            // 
            // XmlSplitter
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1070, 461);
            Controls.Add(logGroupBox);
            Controls.Add(xpathGroupBox);
            Controls.Add(stepsPanel);
            Font = new Font("72", 11.25F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "XmlSplitter";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Bombardier XML Splitting Tool";
            Load += XmlSplitter_Load;
            DragOver += OnDragDrop;
            xmlSelectBox.ResumeLayout(false);
            xmlSelectBox.PerformLayout();
            uowSelectBox.ResumeLayout(false);
            uowSelectBox.PerformLayout();
            stepsPanel.ResumeLayout(false);
            programGroupBox.ResumeLayout(false);
            programGroupBox.PerformLayout();
            outputBox.ResumeLayout(false);
            outputBox.PerformLayout();
            xpathGroupBox.ResumeLayout(false);
            xpathGroupBox.PerformLayout();
            logGroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox xmlSelectBox;
        private GroupBox uowSelectBox;
        private TextBox uowTextBox;
        private Label uowLabel;
        private Button uowButton;
        private TextBox xmlSelectTextBox;
        private Label xmlSelectLabel;
        private Button xmlButton;
        private Panel stepsPanel;
        private GroupBox outputBox;
        private TextBox outDirTextBox;
        private Label dirOutLabel;
        private Button outDirButton;
        private Button execButton;
        private Label outDirWillBeCreated;
        private GroupBox programGroupBox;
        private Label programLabel;
        private ComboBox programsComboBox;
        private GroupBox xpathGroupBox;
        private Label xpathLabel;
        private TextBox xpathTextBox;
        private GroupBox logGroupBox;
        private RichTextBox logTextBox;
    }
}

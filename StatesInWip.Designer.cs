namespace BaXmlSplitter
{
    partial class SelectStates
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
            chooseStatesGroupBox = new GroupBox();
            statesListView = new ListView();
            StateValue = new ColumnHeader();
            StateName = new ColumnHeader();
            StateRemark = new ColumnHeader();
            cancelButton = new Button();
            okButton = new Button();
            chooseStatesGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // chooseStatesGroupBox
            // 
            chooseStatesGroupBox.Controls.Add(statesListView);
            chooseStatesGroupBox.Location = new Point(12, 12);
            chooseStatesGroupBox.Name = "chooseStatesGroupBox";
            chooseStatesGroupBox.Size = new Size(776, 366);
            chooseStatesGroupBox.TabIndex = 0;
            chooseStatesGroupBox.TabStop = false;
            chooseStatesGroupBox.Text = "Hold ctrl and/or shift to select/deselect several states for units of work on which to split";
            // 
            // statesListView
            // 
            statesListView.AccessibleDescription = "List of all distinct states in the manual";
            statesListView.AccessibleName = "Distinct states";
            statesListView.AccessibleRole = AccessibleRole.List;
            statesListView.BorderStyle = BorderStyle.FixedSingle;
            statesListView.CheckBoxes = true;
            statesListView.Columns.AddRange(new ColumnHeader[] { StateValue, StateName, StateRemark });
            statesListView.Dock = DockStyle.Fill;
            statesListView.FullRowSelect = true;
            statesListView.GridLines = true;
            statesListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            statesListView.LabelWrap = false;
            statesListView.Location = new Point(3, 19);
            statesListView.Name = "statesListView";
            statesListView.Size = new Size(770, 344);
            statesListView.Sorting = SortOrder.Ascending;
            statesListView.TabIndex = 0;
            statesListView.UseCompatibleStateImageBehavior = false;
            statesListView.View = View.Details;
            statesListView.OwnerDraw = true;
            statesListView.DrawColumnHeader += ListView_DrawColumnHeader;
            statesListView.DrawItem += ListView_DrawItem;
            statesListView.ItemSelectionChanged += StatesListView_ItemSelectionChanged;
            // 
            // StateValue
            // 
            StateValue.Text = "Value";
            StateValue.TextAlign = HorizontalAlignment.Right;
            StateValue.Width = 84;
            // 
            // StateName
            // 
            StateName.Text = "Name";
            StateName.TextAlign = HorizontalAlignment.Center;
            StateName.Width = 255;
            // 
            // StateRemark
            // 
            StateRemark.Text = "Remark";
            StateRemark.Width = 424;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(700, 387);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(88, 37);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(606, 387);
            okButton.Name = "okButton";
            okButton.Size = new Size(88, 37);
            okButton.TabIndex = 1;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += OkButton_Click;
            // 
            // SelectStates
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 436);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(chooseStatesGroupBox);
            Name = "SelectStates";
            Text = "Select work in progress states";
            Load += StatesInWip_Load;
            chooseStatesGroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox chooseStatesGroupBox;
        private Button cancelButton;
        private Button okButton;
        private ListView statesListView;
        private ColumnHeader StateValue;
        private ColumnHeader StateName;
        private ColumnHeader StateRemark;
    }
}
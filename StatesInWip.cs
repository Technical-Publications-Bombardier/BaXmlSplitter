namespace BaXmlSplitter
{
    internal partial class SelectStates : Form
    {
        private readonly UowState[] allStates;
        public UowState[]? SelectedStates { get; set; }
        public SelectStates(ListViewItem[] items, UowState[] allStates)
        {
            InitializeComponent();
            this.allStates = allStates;
            AcceptButton = okButton;
            CancelButton = cancelButton;
            statesListView.Items.AddRange(items);
            statesListView.Invalidate();
            statesListView.Update();
        }

        private void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
            e.Item.BackColor = (e.ItemIndex % 2) == 0 ? Color.LightSteelBlue : Color.AliceBlue;
            e.Item.UseItemStyleForSubItems = true;
        }

        private void StatesInWip_Load(object sender, EventArgs e)
        {
            statesListView.CreateControl();
            statesListView.Update();
        }
        private void OkButton_Click(object sender, EventArgs e)
        {
            var indices = statesListView.CheckedItems.Cast<ListViewItem>().Where(item => int.TryParse(item.SubItems[0].Text, out var value)).Select(item => int.Parse(item.SubItems[0].Text)).Cast<int>();
            SelectedStates = allStates.Where((UowState stateFromAll) => stateFromAll.StateValue is { } stateValue && indices.Contains(stateValue)).Cast<UowState>().ToArray();
            DialogResult = DialogResult.OK;
            Close();
        }
        private void StatesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            return;
        }

    }
}

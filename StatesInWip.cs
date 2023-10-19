using System.Data;

namespace BaXmlSplitter
{
    internal partial class SelectStates : Form
    {
        private readonly UowState[] AllStates;
        public UowState[]? SelectedStates { get; set; }
        public SelectStates(ListViewItem[] items, UowState[] allStates)
        {
            InitializeComponent();
            AllStates = allStates;
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
            if ((e.ItemIndex % 2) == 0)
            {
                e.Item.BackColor = Color.LightSteelBlue;
            }
            else
            {
                e.Item.BackColor = Color.AliceBlue;
            }
            e.Item.UseItemStyleForSubItems = true;
        }

        private void StatesInWip_Load(object sender, EventArgs e)
        {
            statesListView.CreateControl();
            statesListView.Update();
        }
        private void OkButton_Click(object sender, EventArgs e)
        {
            IEnumerable<int> indices = statesListView.CheckedItems.Cast<ListViewItem>().Where(item => int.TryParse(item.SubItems[0].Text, out int value)).Select(item => int.Parse(item.SubItems[0].Text)).Cast<int>();
            SelectedStates = AllStates.Where((UowState stateFromAll) => stateFromAll.StateValue is int stateValue && indices.Contains(stateValue)).Cast<UowState>().ToArray();
            DialogResult = DialogResult.OK;
            Close();
        }
        private void StatesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            return;
        }

    }
}

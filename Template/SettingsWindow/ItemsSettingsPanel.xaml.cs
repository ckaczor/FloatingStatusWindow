using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Template.SettingsWindow;

public partial class ItemsSettingsPanel
{
    private CollectionViewSource _collectionViewSource;

    public ItemsSettingsPanel()
    {
        InitializeComponent();
    }

    public override string CategoryName => Properties.Resources.optionCategoryItems;

    public override void LoadPanel(Window parentWindow)
    {
        base.LoadPanel(parentWindow);

        if (_collectionViewSource == null)
        {
            _collectionViewSource = new CollectionViewSource { Source = Data.ItemEntries };

            ItemDataGrid.ItemsSource = _collectionViewSource.View;
        }

        _collectionViewSource.View.Refresh();

        if (ItemDataGrid.Items.Count > 0)
            ItemDataGrid.SelectedIndex = 0;

        SetItemButtonStates();
    }

    private void HandleItemDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetItemButtonStates();
    }

    private void SetItemButtonStates()
    {
        AddItemButton.IsEnabled = true;
        EditItemButton.IsEnabled = ItemDataGrid.SelectedItems.Count == 1;
        DeleteItemButton.IsEnabled = ItemDataGrid.SelectedItems.Count > 0;
    }

    private void HandleAddItemButtonClick(object sender, RoutedEventArgs e)
    {
        AddItem();
    }

    private void HandleEditItemButtonClick(object sender, RoutedEventArgs e)
    {
        EditSelectedItem();
    }

    private void HandleDeleteItemButtonClick(object sender, RoutedEventArgs e)
    {
        DeleteSelectedItems();
    }

    private void HandleItemDataGridRowMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        EditSelectedItem();
    }

    private void AddItem()
    {
        var itemEntry = new ItemEntry { };

        var itemWindow = new ItemWindow();

        var result = itemWindow.Display(itemEntry, Window.GetWindow(this));

        if (!result.HasValue || !result.Value)
            return;

        ItemDataGrid.SelectedItem = itemEntry;

        SetItemButtonStates();
    }

    private void EditSelectedItem()
    {
        if (ItemDataGrid.SelectedItem == null)
            return;

        var itemEntry = (ItemEntry)ItemDataGrid.SelectedItem;

        var itemWindow = new ItemWindow();

        itemWindow.Display(itemEntry, Window.GetWindow(this));
    }

    private void DeleteSelectedItems()
    {
        if (MessageBox.Show(ParentWindow!, Properties.Resources.ConfirmDeleteItemss, Properties.Resources.ConfirmDeleteTitle, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
            return;

        var selectedItems = new ItemEntry[ItemDataGrid.SelectedItems.Count];

        ItemDataGrid.SelectedItems.CopyTo(selectedItems, 0);

        foreach (var itemEntry in selectedItems)
            Data.ItemEntries.Remove(itemEntry);

        SetItemButtonStates();
    }
}
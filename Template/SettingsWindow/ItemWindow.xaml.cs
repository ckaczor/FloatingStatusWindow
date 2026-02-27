using System;
using System.Windows;
using ChrisKaczor.Wpf.Validation;

namespace Template.SettingsWindow;

public partial class ItemWindow
{
    public ItemWindow()
    {
        InitializeComponent();
    }

    public bool? Display(ItemEntry itemEntry, Window owner)
    {
        DataContext = itemEntry;

        Title = string.IsNullOrWhiteSpace(itemEntry.Name) ? Properties.Resources.ItemWindowAdd : Properties.Resources.ItemWindowEdit;

        Owner = owner;

        return ShowDialog();
    }

    private void HandleOkayButtonClick(object sender, RoutedEventArgs e)
    {
        if (!this.IsValid())
            return;

        var item = (ItemEntry)DataContext;

        if (!Data.ItemEntries.Contains(item))
            Data.ItemEntries.Add(item);

        Data.Save();

        DialogResult = true;

        Close();
    }
}
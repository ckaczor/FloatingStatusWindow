using System.Collections.ObjectModel;
using System.Text.Json;
using Template.Properties;

namespace Template;

internal static class Data
{
    internal static ObservableCollection<ItemEntry> ItemEntries { get; private set; }

    internal static void Load()
    {
        ItemEntries = JsonSerializer.Deserialize<ObservableCollection<ItemEntry>>(Settings.Default.Items);
    }

    internal static void Save()
    {
        Settings.Default.Items = JsonSerializer.Serialize(ItemEntries);
        Settings.Default.Save();
    }
}
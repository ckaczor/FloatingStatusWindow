using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace Template;

public class ItemEntry : INotifyDataErrorInfo
{
    private readonly DataErrorDictionary _dataErrorDictionary;

    public ItemEntry()
    {
        _dataErrorDictionary = new DataErrorDictionary();
        _dataErrorDictionary.ErrorsChanged += DataErrorDictionaryErrorsChanged;
    }

    public string Name
    {
        get;
        set
        {
            if (!ValidateName(value))
                return;

            field = value;
        }
    }

    [JsonIgnore]
    public bool HasErrors => _dataErrorDictionary.Any();

    
    public IEnumerable GetErrors(string propertyName)
    {
        return _dataErrorDictionary.GetErrors(propertyName);
    }

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    private void DataErrorDictionaryErrorsChanged(object sender, DataErrorsChangedEventArgs e)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(e.PropertyName));
    }

    private bool ValidateName(string newValue)
    {
        _dataErrorDictionary.ClearErrors(nameof(Name));

        if (!string.IsNullOrWhiteSpace(newValue))
            return true;

        _dataErrorDictionary.AddError(nameof(Name), "Name cannot be empty");

        return false;
    }
}
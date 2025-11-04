//@* home branch *@

public class ActiveFilter
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public object? OriginalValue { get; set; }

    public ActiveFilter() { }

    public ActiveFilter(string type, string value, string displayText, object? originalValue = null)
    {
        Type = type;
        Value = value;
        DisplayText = displayText;
        OriginalValue = originalValue;
    }
}
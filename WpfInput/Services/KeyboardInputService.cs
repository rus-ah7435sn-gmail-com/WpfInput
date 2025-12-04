using System.Globalization;
using System.Windows;

namespace KeyboardDemo.PrismUnity.Services;

public sealed class KeyboardInputService : IKeyboardInputService
{
    private ITextInputTarget? _t1;
    private ITextInputTarget? _t2;
    private ITextInputTarget? _t3;
    private ActiveTarget _active;

    public bool IsNumericOnly { get; set; } = true;

    public void AttachTargets(ITextInputTarget t1, ITextInputTarget t2, ITextInputTarget t3)
    {
        _t1 = t1;
        _t2 = t2;
        _t3 = t3;
    }

    public void SetActiveTarget(ActiveTarget target) => _active = target;

    public void SendText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var filtered = IsNumericOnly && (_active == ActiveTarget.Text1 || _active == ActiveTarget.Text2)
            ? FilterNumeric(text)
            : text;

        if (string.IsNullOrEmpty(filtered)) return;

        GetTarget()?.InsertText(filtered);
    }

    public void Backspace() => GetTarget()?.Backspace();
    public void Delete() => GetTarget()?.Delete();
    public void Clear() => GetTarget()?.Clear();

    public void PasteFromClipboard()
    {
        if (!Clipboard.ContainsText()) return;
        var text = Clipboard.GetText();
        SendText(text);
    }

    private ITextInputTarget? GetTarget() => _active switch
    {
        ActiveTarget.Text1 => _t1,
        ActiveTarget.Text2 => _t2,
        ActiveTarget.Text3 => _t3,
        _ => _t3 // fallback: мы хотим, чтобы “не туда” всё равно шло в Text3
    };

    private static string FilterNumeric(string text)
    {
        // “всё, что связано с цифровым вводом”
        // Разрешаем: цифры, + -, пробел, десятичные . , и локальный decimal separator.
        var dec = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var result = new char[text.Length];
        var n = 0;

        for (int i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsDigit(c) || c == '+' || c == '-' || c == ' ' || c == '.' || c == ',' || dec.Contains(c))
                result[n++] = c;
        }

        return n == 0 ? string.Empty : new string(result, 0, n);
    }
}

using System.Windows.Controls;

namespace KeyboardDemo.PrismUnity.Services;

public sealed class TextBoxInputTarget : ITextInputTarget
{
    private readonly TextBox _tb;
    private readonly bool _appendOnly;

    public TextBoxInputTarget(TextBox tb, bool appendOnly)
    {
        _tb = tb;
        _appendOnly = appendOnly;
    }

    public void InsertText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (_appendOnly)
        {
            _tb.Text = (_tb.Text ?? string.Empty) + text;
            _tb.CaretIndex = _tb.Text.Length;
            _tb.SelectionLength = 0;
            return;
        }

        var original = _tb.Text ?? string.Empty;
        var start = _tb.SelectionStart;
        var len = _tb.SelectionLength;

        var next = original.Remove(start, len).Insert(start, text);
        _tb.Text = next;
        _tb.CaretIndex = start + text.Length;
        _tb.SelectionLength = 0;
    }

    public void Backspace()
    {
        var s = _tb.Text ?? string.Empty;
        if (s.Length == 0) return;

        if (_appendOnly)
        {
            _tb.Text = s[..^1];
            _tb.CaretIndex = _tb.Text.Length;
            return;
        }

        if (_tb.SelectionLength > 0)
        {
            Delete();
            return;
        }

        var i = _tb.CaretIndex;
        if (i <= 0) return;

        _tb.Text = s.Remove(i - 1, 1);
        _tb.CaretIndex = i - 1;
    }

    public void Delete()
    {
        var s = _tb.Text ?? string.Empty;
        if (s.Length == 0) return;

        if (_appendOnly)
        {
            Backspace();
            return;
        }

        if (_tb.SelectionLength > 0)
        {
            var start = _tb.SelectionStart;
            _tb.Text = s.Remove(start, _tb.SelectionLength);
            _tb.CaretIndex = start;
            _tb.SelectionLength = 0;
            return;
        }

        var i = _tb.CaretIndex;
        if (i < 0 || i >= s.Length) return;

        _tb.Text = s.Remove(i, 1);
        _tb.CaretIndex = i;
    }

    public void Clear()
    {
        _tb.Text = string.Empty;
        _tb.CaretIndex = 0;
        _tb.SelectionLength = 0;
    }

    public void SetCaretToEnd()
    {
        _tb.CaretIndex = (_tb.Text ?? string.Empty).Length;
        _tb.SelectionLength = 0;
    }
}

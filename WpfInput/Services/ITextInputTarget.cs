namespace KeyboardDemo.PrismUnity.Services;

public interface ITextInputTarget
{
    void InsertText(string text);
    void Backspace();
    void Delete();
    void Clear();
    void SetCaretToEnd();
}

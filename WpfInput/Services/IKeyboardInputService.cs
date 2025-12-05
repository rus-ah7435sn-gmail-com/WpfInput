namespace KeyboardDemo.PrismUnity.Services;

public enum ActiveTarget
{
    None,
    Text1,
    Text2,
    Text3
}

public interface IKeyboardInputService
{
    bool IsNumericOnly { get; set; }

    event EventHandler? EnterRequested;
    event EventHandler? CloseKeyboardRequested;

    void AttachTargets(ITextInputTarget t1, ITextInputTarget t2, ITextInputTarget t3);
    void SetActiveTarget(ActiveTarget target);

    void SendText(string text);
    void Backspace();
    void Delete();
    void Clear();
    void PasteFromClipboard();

    void RequestEnter();
    void RequestCloseKeyboard();
}

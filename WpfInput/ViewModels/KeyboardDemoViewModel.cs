using Prism.Commands;
using Prism.Mvvm;
using KeyboardDemo.PrismUnity.Services;

namespace KeyboardDemo.PrismUnity.ViewModels;

public sealed class KeyboardDemoViewModel : BindableBase
{
    private readonly IKeyboardInputService _keyboard;

    public KeyboardDemoViewModel(IKeyboardInputService keyboard)
    {
        _keyboard = keyboard;

        PressKeyCommand = new DelegateCommand<string?>(OnPressKey);
    }

    private string _text1 = "";
    private string _text2 = "";
    private string _text3 = "";
    private bool _isNumericOnly = true;

    public string Text1 { get => _text1; set => SetProperty(ref _text1, value); }
    public string Text2 { get => _text2; set => SetProperty(ref _text2, value); }
    public string Text3 { get => _text3; set => SetProperty(ref _text3, value); }

    public bool IsNumericOnly
    {
        get => _isNumericOnly;
        set
        {
            if (SetProperty(ref _isNumericOnly, value))
                _keyboard.IsNumericOnly = value;
        }
    }

public DelegateCommand<string?> PressKeyCommand { get; }

private void OnPressKey(string? key)
{
    if (string.IsNullOrEmpty(key)) return;

    switch (key)
    {
        case "ENTER":
            _keyboard.RequestEnter();
            break;
        case "ESC":
            _keyboard.Clear();
            _keyboard.RequestCloseKeyboard();
            break;
            case "BACK":
            _keyboard.Backspace();
            break;
        default:
            _keyboard.SendText(key);
            break;
    }
}
}

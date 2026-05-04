using KeyboardDemo.PrismUnity.Services;
using KeyboardDemo.PrismUnity.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;

namespace KeyboardDemo.PrismUnity.Views;

public partial class KeyboardDemoView : UserControl
{
    private readonly IKeyboardInputService _keyboard;
    private ITextInputTarget? _t1;
    private ITextInputTarget? _t2;
    private ITextInputTarget? _t3;

    private void FocusSink()
    {
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            if (XamlRoot is null) return;
            var focused = FocusManager.GetFocusedElement(XamlRoot);
            if (IsUiControlElement(focused)) return;
            InputSink.Focus(FocusState.Programmatic);
        });
    }

    public KeyboardDemoView() : this(App.Services.GetRequiredService<IKeyboardInputService>()) { }

    public KeyboardDemoView(IKeyboardInputService keyboard)
    {
        InitializeComponent();
        _keyboard = keyboard;
        DataContext = App.Services.GetRequiredService<KeyboardDemoViewModel>();

        _keyboard.EnterRequested += (_, _) =>
        {
            if (IsFocusInText1OrText2(out _))
            {
                HideKeyboard();
                _keyboard.SetActiveTarget(ActiveTarget.Text3);
                FocusSink();
            }
        };

        _keyboard.CloseKeyboardRequested += (_, _) =>
        {
            HideKeyboard();
            _keyboard.SetActiveTarget(ActiveTarget.Text3);
        };

        Loaded += OnLoaded;

        AddHandler(UIElement.CharacterReceivedEvent,
            new TypedEventHandler<UIElement, CharacterReceivedRoutedEventArgs>(OnCharacterReceived), true);
        AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown), true);
        // GotFocusEvent не экспонируется как статическое поле в WinUI 3 — подписываемся напрямую
        GotFocus += OnGotFocus;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _t1 = new TextBoxInputTarget(Text1Box, appendOnly: false);
        _t2 = new TextBoxInputTarget(Text2Box, appendOnly: false);
        _t3 = new TextBoxInputTarget(Text3Box, appendOnly: true);

        _keyboard.AttachTargets(_t1, _t2, _t3);
        _keyboard.SetActiveTarget(ActiveTarget.Text3);
        FocusSink();

        if (DataContext is KeyboardDemoViewModel vm)
        {
            _keyboard.IsNumericOnly = vm.IsNumericOnly;
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(KeyboardDemoViewModel.IsNumericOnly))
                    _keyboard.IsNumericOnly = vm.IsNumericOnly;
            };
        }
    }

    private static bool IsUiControlElement(object? el)
    {
        if (el is not DependencyObject d) return false;
        DependencyObject? current = d;
        while (current is not null)
        {
            if (current is CheckBox or ButtonBase or ComboBox or Slider or ToggleButton)
                return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        var source = e.OriginalSource;

        if (IsSameOrChild(source, Text1Box))
        {
            ShowKeyboard(Text1Box);
            _keyboard.SetActiveTarget(ActiveTarget.Text1);
            return;
        }

        if (IsSameOrChild(source, Text2Box))
        {
            ShowKeyboard(Text2Box);
            _keyboard.SetActiveTarget(ActiveTarget.Text2);
            return;
        }

        HideKeyboard();
        _keyboard.SetActiveTarget(ActiveTarget.Text3);
        if (!IsUiControlElement(source))
            FocusSink();
    }

    private void OnCharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs e)
    {
        if (IsFocusInText1OrText2(out var tb))
        {
            _keyboard.SetActiveTarget(tb == Text1Box ? ActiveTarget.Text1 : ActiveTarget.Text2);
            _keyboard.SendText(e.Character.ToString());
            e.Handled = true;
            return;
        }

        _keyboard.SetActiveTarget(ActiveTarget.Text3);
        _keyboard.SendText(e.Character.ToString());
        e.Handled = true;
    }

    private async void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        var ctrlDown = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
                           .HasFlag(CoreVirtualKeyStates.Down);

        if (ctrlDown && e.Key == VirtualKey.V)
        {
            RouteTargetByFocus();
            await _keyboard.PasteFromClipboardAsync();
            e.Handled = true;
            return;
        }

        if (e.Key == VirtualKey.Back)
        {
            RouteTargetByFocus();
            _keyboard.Backspace();
            e.Handled = true;
            return;
        }

        if (e.Key == VirtualKey.Delete)
        {
            RouteTargetByFocus();
            _keyboard.Delete();
            e.Handled = true;
            return;
        }

        if (e.Key == VirtualKey.Escape)
        {
            RouteTargetByFocus();
            _keyboard.Clear();
            HideKeyboard();
            e.Handled = true;
            return;
        }

        if (e.Key == VirtualKey.Enter || e.Key == VirtualKey.Tab)
        {
            if (IsFocusInText1OrText2(out _))
                HideKeyboard();
            e.Handled = true;
        }
    }

    private void RouteTargetByFocus()
    {
        if (IsFocusInText1OrText2(out var tb))
            _keyboard.SetActiveTarget(tb == Text1Box ? ActiveTarget.Text1 : ActiveTarget.Text2);
        else
            _keyboard.SetActiveTarget(ActiveTarget.Text3);
    }

    private bool IsFocusInText1OrText2(out TextBox target)
    {
        target = null!;
        if (XamlRoot is null) return false;
        var focused = FocusManager.GetFocusedElement(XamlRoot);
        if (IsSameOrChild(focused, Text1Box)) { target = Text1Box; return true; }
        if (IsSameOrChild(focused, Text2Box)) { target = Text2Box; return true; }
        return false;
    }

    private static bool IsSameOrChild(object? element, FrameworkElement parent)
    {
        if (element is not DependencyObject d) return false;
        DependencyObject? current = d;
        while (current is not null)
        {
            if (current == parent) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    private void ShowKeyboard(TextBox placementTarget)
    {
        PositionPopup(placementTarget);
        if (!KeyboardPopup.IsOpen)
            KeyboardPopup.IsOpen = true;
    }

    private void HideKeyboard()
    {
        if (KeyboardPopup.IsOpen)
            KeyboardPopup.IsOpen = false;
        _keyboard.SetActiveTarget(ActiveTarget.Text3);
        FocusSink();
    }

    private void PositionPopup(TextBox target)
    {
        const double popupHeight = 320;
        const double gap = 4;

        var transform = target.TransformToVisual(ContentGrid);
        var pos = transform.TransformPoint(new Point(0, 0));

        double x = pos.X;
        double y = pos.Y + target.ActualHeight + gap;

        // Если снизу нет места — показать сверху
        if (y + popupHeight > ContentGrid.ActualHeight && pos.Y - popupHeight - gap >= 0)
            y = pos.Y - popupHeight - gap;

        KeyboardPopup.HorizontalOffset = x;
        KeyboardPopup.VerticalOffset   = y;
    }
}

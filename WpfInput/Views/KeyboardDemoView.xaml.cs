using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using KeyboardDemo.PrismUnity.Services;
using KeyboardDemo.PrismUnity.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KeyboardDemo.PrismUnity.Views;

public partial class KeyboardDemoView : UserControl
{
    private readonly IKeyboardInputService _keyboard;
    private ITextInputTarget? _t1;
    private ITextInputTarget? _t2;
    private ITextInputTarget? _t3;

    private void FocusSink()
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var focused = Keyboard.FocusedElement;
            if (IsUiControlElement(focused))
                return;

            InputSink.Focus();
            Keyboard.Focus(InputSink);

        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    public KeyboardDemoView()
    : this(ResolveKeyboardService())
    {
    }

    public KeyboardDemoView(IKeyboardInputService keyboard)
    {
        InitializeComponent();

        _keyboard = keyboard;

        if (!DesignerProperties.GetIsInDesignMode(this))
            DataContext = App.Services.GetRequiredService<KeyboardDemoViewModel>();

        _keyboard.EnterRequested += (_, __) =>
        {
            if (IsFocusInText1OrText2(out var _))
            {
                Keyboard.ClearFocus();
                HideKeyboard();

                _keyboard.SetActiveTarget(ActiveTarget.Text3);
                FocusSink();
            }
        };

        _keyboard.CloseKeyboardRequested += (_, __) =>
        {
            Keyboard.ClearFocus();
            HideKeyboard();
            _keyboard.SetActiveTarget(ActiveTarget.Text3);
        };

        Loaded += OnLoaded;

        AddHandler(PreviewTextInputEvent, new TextCompositionEventHandler(OnPreviewTextInput), true);
        AddHandler(PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
        AddHandler(Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus), true);
    }

    private static IKeyboardInputService ResolveKeyboardService()
    {
        if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            return new KeyboardInputService();

        return App.Services.GetRequiredService<IKeyboardInputService>();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _t1 = new TextBoxInputTarget(Text1Box, appendOnly: false);
        _t2 = new TextBoxInputTarget(Text2Box, appendOnly: false);
        _t3 = new TextBoxInputTarget(Text3Box, appendOnly: true);

        _keyboard.AttachTargets(_t1, _t2, _t3);
        _keyboard.SetActiveTarget(ActiveTarget.Text3);
        FocusSink();

        KeyboardPopup.CustomPopupPlacementCallback = PlaceKeyboardPopup;

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

    private bool IsUiControlElement(IInputElement? el)
    {
        if (el is null) return false;

        DependencyObject? d = el as DependencyObject;

        while (d != null)
        {
            if (d is CheckBox || d is ButtonBase || d is ComboBox || d is Slider || d is ToggleButton)
                return true;

            d = VisualTreeHelper.GetParent(d);
        }

        return false;
    }

    private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (IsFocusInText1OrText2(out var tb))
        {
            ShowKeyboard(tb);
            _keyboard.SetActiveTarget(tb == Text1Box ? ActiveTarget.Text1 : ActiveTarget.Text2);
        }
        else
        {
            HideKeyboard();
            _keyboard.SetActiveTarget(ActiveTarget.Text3);
            if (!IsUiControlElement(e.NewFocus))
                FocusSink();
        }
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (IsFocusInText1OrText2(out var tb))
        {
            _keyboard.SetActiveTarget(tb == Text1Box ? ActiveTarget.Text1 : ActiveTarget.Text2);
            _keyboard.SendText(e.Text);
            e.Handled = true;
            return;
        }

        _keyboard.SetActiveTarget(ActiveTarget.Text3);
        _keyboard.SendText(e.Text);
        e.Handled = true;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.V)
        {
            if (IsFocusInText1OrText2(out var tb))
                _keyboard.SetActiveTarget(tb == Text1Box ? ActiveTarget.Text1 : ActiveTarget.Text2);
            else
                _keyboard.SetActiveTarget(ActiveTarget.Text3);

            _keyboard.PasteFromClipboard();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Back)
        {
            RouteTargetByFocus();
            _keyboard.Backspace();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Delete)
        {
            RouteTargetByFocus();
            _keyboard.Delete();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            RouteTargetByFocus();
            _keyboard.Clear();
            HideKeyboard();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter || e.Key == Key.Tab)
        {
            if (IsFocusInText1OrText2(out _))
            {
                Keyboard.ClearFocus();
                HideKeyboard();
            }

            e.Handled = true;
            return;
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

        if (Text1Box.IsKeyboardFocusWithin) { target = Text1Box; return true; }
        if (Text2Box.IsKeyboardFocusWithin) { target = Text2Box; return true; }

        return false;
    }

    private void ShowKeyboard(TextBox placementTarget)
    {
        KeyboardPopup.PlacementTarget = placementTarget;
        if (!KeyboardPopup.IsOpen)
            KeyboardPopup.IsOpen = true;

        KeyboardPopup.HorizontalOffset += 0.1;
        KeyboardPopup.HorizontalOffset -= 0.1;
    }

    private void HideKeyboard()
    {
        if (KeyboardPopup.IsOpen)
            KeyboardPopup.IsOpen = false;
        _keyboard.SetActiveTarget(ActiveTarget.Text3);
        FocusSink();
    }

    private CustomPopupPlacement[] PlaceKeyboardPopup(Size popupSize, Size targetSize, Point offset)
    {
        var target = KeyboardPopup.PlacementTarget as FrameworkElement;
        if (target == null)
            return new[] { new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Horizontal) };

        var topLeft = target.PointToScreen(new Point(0, 0));
        var work = SystemParameters.WorkArea;

        var spaceBelow = (work.Bottom) - (topLeft.Y + targetSize.Height);
        var fitsBelow = spaceBelow >= popupSize.Height;

        var spaceAbove = (topLeft.Y) - (work.Top);
        var fitsAbove = spaceAbove >= popupSize.Height;

        var below = new CustomPopupPlacement(new Point(0, targetSize.Height), PopupPrimaryAxis.Horizontal);
        var above = new CustomPopupPlacement(new Point(0, -popupSize.Height), PopupPrimaryAxis.Horizontal);

        if (fitsBelow) return new[] { below };
        if (fitsAbove) return new[] { above };
        return new[] { below };
    }
}

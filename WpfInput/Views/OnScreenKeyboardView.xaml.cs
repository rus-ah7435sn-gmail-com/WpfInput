using System.Windows;
using System.Windows.Controls;

namespace KeyboardDemo.PrismUnity.Views;

public partial class OnScreenKeyboardView : UserControl
{
    public OnScreenKeyboardView() => InitializeComponent();

    public bool IsNumericOnly
    {
        get => (bool)GetValue(IsNumericOnlyProperty);
        set => SetValue(IsNumericOnlyProperty, value);
    }

    public static readonly DependencyProperty IsNumericOnlyProperty =
        DependencyProperty.Register(nameof(IsNumericOnly), typeof(bool), typeof(OnScreenKeyboardView),
            new PropertyMetadata(true));
}

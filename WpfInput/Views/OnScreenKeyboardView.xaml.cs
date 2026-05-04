using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KeyboardDemo.PrismUnity.Views;

public partial class OnScreenKeyboardView : UserControl
{
    public OnScreenKeyboardView()
    {
        InitializeComponent();
        Loaded += (_, _) => VisualStateManager.GoToState(this, IsNumericOnly ? "NumericMode" : "FullMode", false);
    }

    public bool IsNumericOnly
    {
        get => (bool)GetValue(IsNumericOnlyProperty);
        set => SetValue(IsNumericOnlyProperty, value);
    }

    public static readonly DependencyProperty IsNumericOnlyProperty =
        DependencyProperty.Register(nameof(IsNumericOnly), typeof(bool), typeof(OnScreenKeyboardView),
            new PropertyMetadata(true, static (d, e) =>
                VisualStateManager.GoToState((OnScreenKeyboardView)d, (bool)e.NewValue ? "NumericMode" : "FullMode", true)));
}

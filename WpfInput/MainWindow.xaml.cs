using System.Windows;
using System.Windows.Input;

namespace KeyboardDemo.PrismUnity;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, __) =>
        {
            Activate();
            Focus();
            Keyboard.ClearFocus();
        };
    }
}

using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace KeyboardDemo.PrismUnity;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(900, 600));
    }
}

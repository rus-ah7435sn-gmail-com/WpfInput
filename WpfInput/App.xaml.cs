using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace KeyboardDemo.PrismUnity;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Services = new ServiceCollection().BuildServiceProvider();
        new MainWindow().Activate();
    }
}

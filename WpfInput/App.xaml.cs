using System.Windows;
using KeyboardDemo.PrismUnity.Services;
using KeyboardDemo.PrismUnity.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KeyboardDemo.PrismUnity;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddSingleton<IKeyboardInputService, KeyboardInputService>();
        services.AddTransient<KeyboardDemoViewModel>();
        Services = services.BuildServiceProvider();

        new MainWindow().Show();
    }
}

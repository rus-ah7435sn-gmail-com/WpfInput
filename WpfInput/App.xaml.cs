using System.Windows;
using KeyboardDemo.PrismUnity.Services;
using Prism.Ioc;
using Prism.Unity;

namespace KeyboardDemo.PrismUnity;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
        => Container.Resolve<MainWindow>();

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IKeyboardInputService, KeyboardInputService>();
    }
}

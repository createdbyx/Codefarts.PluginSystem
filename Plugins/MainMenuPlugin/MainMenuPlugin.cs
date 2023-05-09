using System.Diagnostics;
using Codefarts.DependencyInjection;
using Codefarts.PluginSystem;
using Codefarts.PluginSystemDemo.Models;

namespace MainMenuPlugin;

public class MainMenuPlugin : IPlugin<Application>
{
    private Application application;
    private MenuItem fileMenu;
    private MenuItem exceptionMenuItem;
    private MenuItem disconnectexceptionMenuItem;
    private MenuItem exitMenuItem;

    private IDependencyInjectionProvider diProvider;

    public MainMenuPlugin(IDependencyInjectionProvider diProvider)
    {
        this.diProvider = diProvider ?? throw new ArgumentNullException(nameof(diProvider));
    }

    public void Connect(Application model)
    {
        this.application = model ?? throw new ArgumentNullException(nameof(model));
        this.fileMenu = this.application.MainMenu.FirstOrDefault(x => x.Title == "_File") ?? new MenuItem { Title = "_File" };

        exceptionMenuItem = new MenuItem { Title = "_Throw Exception" };
        exceptionMenuItem.Selected += (_, _) => { throw new Exception("Thrown exception from plugin!"); };

        disconnectexceptionMenuItem = new MenuItem { Title = "_Disconnect" };
        disconnectexceptionMenuItem.Selected += (s, e) =>
        {
            // remove the plugin from the plugins collection
            this.application.Plugins.Remove(this);
    
            // call disconnect
            this.Disconnect();


            var pluginSystem = this.diProvider.Resolve<PluginSystem>();
            pluginSystem.UnloadPlugin(this);
        };

        exitMenuItem = new MenuItem { Title = "E_xit" };
        exitMenuItem.Selected += (_, _) => Debug.WriteLine("Exiting application...");

        fileMenu.SubMenus.Add(exceptionMenuItem);
        fileMenu.SubMenus.Add(disconnectexceptionMenuItem);
        fileMenu.SubMenus.Add(exitMenuItem);
        if (!this.application.MainMenu.Any(x => x.Title == "_File"))
        {
            this.application.MainMenu.Add(fileMenu);
        }
    }

    public void Disconnect()
    {
        this.fileMenu.SubMenus.Remove(this.exceptionMenuItem);
        this.fileMenu.SubMenus.Remove(this.disconnectexceptionMenuItem);
        this.fileMenu.SubMenus.Remove(this.exitMenuItem);

        // cleanup first
        if (this.fileMenu.SubMenus.Count == 0)
        {
            this.application.MainMenu.Remove(this.fileMenu);
        }

        this.application = null;
    } 
}
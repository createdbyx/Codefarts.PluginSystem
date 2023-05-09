using System.Diagnostics;
using System.Runtime.Loader;
using Codefarts.DependencyInjection;
using Codefarts.PluginSystem;
using Codefarts.PluginSystemDemo.Models;

namespace MainMenuPlugin;

public class MainMenuPluigin : IPlugin<Application>
{
    private Application application;
    private MenuItem fileMenu;
    private MenuItem exceptionMenuItem;
    private MenuItem disconnectexceptionMenuItem;
    private MenuItem exitMenuItem;

    private IDependencyInjectionProvider diProvider;

    public MainMenuPluigin(IDependencyInjectionProvider diProvider)
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
            //   UnloadPlugin(this);
            //this.Disconnect();
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

    public void UnloadPlugin(IPlugin<Application> plugin)
    {
        // remove the plugin from the plugins collection
        this.application.Plugins.Remove(plugin);

        // find the assembly load context for the plugin but exclude the Default context
        var context = AssemblyLoadContext.All.FirstOrDefault(x => x.Assemblies.Any(asm => asm.Equals(plugin.GetType().Assembly)));

        // check how many other plugins are using the same context
        var count = this.application.Plugins.Count(x => x.GetType().Assembly.Equals(context.Assemblies.FirstOrDefault()));

        // if there are no other plugins using the same context then unload it
        if (count == 0)
        {
            context.Unload();
        }
    }
}
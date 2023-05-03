using System.Diagnostics;
using System.Runtime.Loader;
using Codefarts.PluginSystemDemo.Models;

namespace MainMenuPlugin;

public class TestPluigin : IPlugin<Application>
{
    private Application application;
    private MenuItem fileMenu;

    public void Connect(Application model)
    {
        this.application = model ?? throw new ArgumentNullException(nameof(model));
        this.fileMenu =this.application.MainMenu.FirstOrDefault(x=>x.Title=="_File") ??  new MenuItem { Title = "_File" };

        var exceptionMenuItem = new MenuItem { Title = "_Throw Exception" };
        exceptionMenuItem.Selected += (_, _) => { throw new Exception("Thrown exception from plugin!"); };

        var disconnectexceptionMenuItem = new MenuItem { Title = "_Disconnect" };
        disconnectexceptionMenuItem.Selected += (s, e) =>
        {
            this.UnloadPlugin(this);
            this.Disconnect();
        };

        var exitMenuItem = new MenuItem { Title = "E_xit" };
        exitMenuItem.Selected += (_, _) => Debug.WriteLine("Exiting application...");

        this.fileMenu.SubMenus.Add(exceptionMenuItem);
        this.fileMenu.SubMenus.Add(disconnectexceptionMenuItem);
        this.fileMenu.SubMenus.Add(exitMenuItem);
        this.application.MainMenu.Add(this.fileMenu);
    }

    public void Disconnect()
    {
        // cleanup first
        this.application.MainMenu.Remove(this.fileMenu);
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
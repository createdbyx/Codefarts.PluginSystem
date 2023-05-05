using System.Diagnostics;
using System.Runtime.Loader;
using Codefarts.PluginSystemDemo.Models;

namespace MainMenuPlugin;

public class InternalTestPluigin : IPlugin<Application>
{
    private Application application;
    private MenuItem fileMenu;
    private MenuItem exceptionMenuItem;

    public void Connect(Application model)
    {
        this.application = model ?? throw new ArgumentNullException(nameof(model));
        this.fileMenu = this.application.MainMenu.FirstOrDefault(x => x.Title == "_File") ?? new MenuItem { Title = "_File" };

        exceptionMenuItem = new MenuItem { Title = "Intenal Te_st" };
        exceptionMenuItem.Selected += (_, _) => { Debug.WriteLine("Internal"); };


        this.fileMenu.SubMenus.Add(exceptionMenuItem);
        if (!this.application.MainMenu.Any(x => x.Title == "_File"))
        {
            this.application.MainMenu.Add(fileMenu);
        }
    }

    public void Disconnect()
    {
        this.fileMenu.SubMenus.Remove(this.exceptionMenuItem);

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
using System.Diagnostics;
using System.Runtime.Loader;
using Codefarts.PluginSystem;
using Codefarts.PluginSystemDemo.Models;

namespace MainMenuPlugin;

public class MainMenuPluigin : IPlugin<Application>
{
    private Application application;
    private MenuItem fileMenu;

    public void Connect(Application model)
    {
        this.application = model;
        this.fileMenu = new MenuItem { Title = "_File" };

        var exceptionMenuItem = new MenuItem { Title = "_Throw Exception" };
        exceptionMenuItem.Selected += (s, e) => { throw new Exception("Thrown exception from plugin!"); };

        var disconnectexceptionMenuItem = new MenuItem { Title = "_Disconnect" };
        disconnectexceptionMenuItem.Selected += (s, e) =>
        {
            this.Disconnect();
        };

        var exitMenuItem = new MenuItem { Title = "E_xit" };
        exitMenuItem.Selected += (s, e) => Debug.WriteLine("Exiting application...");

        fileMenu.SubMenus.Add(exceptionMenuItem);
        fileMenu.SubMenus.Add(disconnectexceptionMenuItem);
        fileMenu.SubMenus.Add(exitMenuItem);
        this.application.MainMenu.Add(fileMenu);
    }

    public void Disconnect()
    {
        // cleanup first
        this.application.MainMenu.Remove(this.fileMenu);
        this.application = null;
    }
}
using System.Diagnostics;
using Codefarts.PluginSystemDemo.Models;

namespace MainMenuPlugin;

public class InternalTestPlugin : IPlugin<Application>
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
}
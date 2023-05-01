using Codefarts.PluginSystemDemo.Models.Collections;

namespace Codefarts.PluginSystemDemo.Models;

public class Application : BaseClass
{
    private string title;
    private MenuItemCollection mainMenu;
    private PluginCollection plugins;

    public PluginCollection Plugins
    {
        get
        {
            return this.plugins;
        }

        private set
        {
            this.SetField(ref this.plugins, value);
        }
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="Application" /> class.
    /// </summary>
    public Application()
    {
        MainMenu = new MenuItemCollection();
        this.plugins = new PluginCollection();
    }

    public MenuItemCollection MainMenu
    {
        get
        {
            return this.mainMenu;
        }

        set
        {
            this.SetField(ref this.mainMenu, value);
        }
    }

    public string Title
    {
        get
        {
            return this.title;
        }

        set
        {
            this.SetField(ref this.title, value);
        }
    }
}
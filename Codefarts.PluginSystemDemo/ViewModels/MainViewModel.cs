using System;
using System.IO;
using System.Windows.Input;
using Codefarts.DependencyInjection;
using Codefarts.DependencyInjection.CodefartsIoc;
using Codefarts.PluginSystemDemo.Models;
using Codefarts.WPFCommon.Commands;

namespace Codefarts.PluginSystemDemo.ViewModels;

public class MainViewModel : BaseClass
{
    private Application application;
    private IDependencyInjectionProvider diProvider;
    private PluginSystem.PluginSystem pluginSystem;

    public MainViewModel()
    {
        this.diProvider = new DependencyInjectorShim();
        this.diProvider.Register<IDependencyInjectionProvider>(() => this.diProvider);

        this.application = this.diProvider.Resolve<Application>();
        this.diProvider.Register<Application>(() => this.application);

        this.pluginSystem = this.diProvider.Resolve<PluginSystem.PluginSystem>();
        this.diProvider.Register<PluginSystem.PluginSystem>(() => this.pluginSystem);
        
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        var files = Directory.GetFiles(path, "*.plugin", SearchOption.AllDirectories);

        //.Select(x => Path.ChangeExtension(x, ".dll")).ToList());
        this.pluginSystem.LoadPlugins<IPlugin<Application>>(files, type =>
        {
            // create the plugin using the dependency injection provider
            var plugin = this.diProvider.Resolve(type) as IPlugin<Application>;

            // add the plugin to the plugins collection
            this.application.Plugins.Add(plugin);

            // call connect on the plugin
            plugin.Connect(this.application);

            return plugin;
        });
    }

    public ICommand RunMenuItem
    {
        get
        {
            return new DelegateCommand(null, parameter =>
            {
                var item = parameter as MenuItem;
                //Dispatcher.CurrentDispatcher.Invoke(() => item?.Select());
                item?.Select();
            });
        }
    }

    public Application Application
    {
        get
        {
            return this.application;
        }

        set
        {
            this.SetField(ref this.application, value);
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Codefarts.DependencyInjection;
using Codefarts.DependencyInjection.CodefartsIoc;
using Codefarts.PluginSystem;
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
        var paths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"),
            AppDomain.CurrentDomain.BaseDirectory,
        };
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        var files = Directory.GetFiles(path, "*.plugin", SearchOption.AllDirectories);
        var reader = this.diProvider.Resolve<PluginReader>();
        var results = files.SelectMany(f => reader.Read(f)).ToArray();

        //.Select(x => Path.ChangeExtension(x, ".dll")).ToList());
        this.pluginSystem.LoadPlugins<IPlugin<Application>>(files);
    }

    // private ICommand PluginCreated
    // {
    //     get
    //     {
    //         return new GenericDelegateCommand<Type>(p => !this.application.Plugins.Any(x => x.GetType().Equals(p)),
    //                                                 type =>
    //                                                 {
    //                                                     var plugin = this.diProvider.Resolve(type) as IPlugin<Application>;
    //                                                     this.application.Plugins.Add(plugin);
    //                                                     plugin.Connect(this.application);
    //                                                 });
    //     }
    // }

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
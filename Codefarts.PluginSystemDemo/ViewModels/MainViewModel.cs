using System;
using System.Linq;
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
    private PluginSystem pluginSystem;

    public MainViewModel()
    {
        this.diProvider = new DependencyInjectorShim();
        this.application = this.diProvider.Resolve<Application>();

        this.pluginSystem = this.diProvider.Resolve<PluginSystem>();
        this.pluginSystem.LoadPlugins(PluginCreated);
    }

    private ICommand PluginCreated
    {
        get
        {
            return new GenericDelegateCommand<Type>(p => !this.application.Plugins.Any(x => x.GetType().Equals(p)),
                                                    type =>
                                                    {
                                                        var plugin = this.diProvider.Resolve(type) as IPlugin<Application>;
                                                        this.application.Plugins.Add(plugin);
                                                        plugin.Connect(this.application);
                                                    });
        }
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
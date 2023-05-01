using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using System.Windows.Input;
using System.Windows.Threading;
using Codefarts.DependencyInjection;
using Codefarts.DependencyInjection.CodefartsIoc;
using Codefarts.PluginSystemDemo.Models;
using Codefarts.WPFCommon.Commands;

namespace Codefarts.PluginSystem.ViewModels;

public class MainViewModel : BaseClass
{
    private Application application;
    private IDependencyInjectionProvider diProvider;

    public MainViewModel()
    {
        this.diProvider = new DependencyInjectorShim();
        this.application = new Application();

        // load plugins
        DoLoadPlugins();
    }

    public ICommand RunMenuItem
    {
        get
        {
            return new DelegateCommand(null, (parameter) =>
            {
                var item = parameter as MenuItem;
                //Dispatcher.CurrentDispatcher.Invoke(() => item?.Select());
                item?.Select();
            });
        }
    }

    private void DoLoadPlugins()
    {
        var path = Path.Combine(Environment.CurrentDirectory, "Plugins");
        var files = Directory.GetFiles(path, "*.plugin", SearchOption.AllDirectories)
                             .Select(x => Path.ChangeExtension(x, ".dll"));
        var pluginContexts = new Dictionary<string, AssemblyLoadContext>();

        Func<string, AssemblyLoadContext> contextCallback = (file) =>
        {
            var context = new AssemblyLoadContext(file, true);
            context.Resolving += (context, name) =>
            {
                var assembly = context.LoadFromAssemblyName(name);
                return assembly;
            };
            pluginContexts.Add(file, context);
            return context;
        };

        var finder = Codefarts.TypeLocator.TypeLocator.FindPublicClassesThatAreAssagnableTo<IPlugin<Application>>(files, contextCallback);

        // create types
        foreach (var item in finder)
        {
            var plugin = diProvider.Resolve(item) as IPlugin<Application>;
            this.application.Plugins.Add(plugin);
            plugin.Connect(this.application);
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
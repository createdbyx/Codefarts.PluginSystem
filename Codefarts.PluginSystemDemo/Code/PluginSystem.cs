using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Windows.Input;
using Codefarts.PluginSystemDemo.Models;

namespace Codefarts.PluginSystemDemo;

public class PluginSystem
{
    private Dictionary<string, AssemblyLoadContext> pluginContexts;

    public IReadOnlyDictionary<string, AssemblyLoadContext> PluginContexts
    {
        get
        {
            return new ReadOnlyDictionary<string, AssemblyLoadContext>(this.pluginContexts);
        }
    }

    public PluginSystem() 
    {
        pluginContexts = new Dictionary<string, AssemblyLoadContext>();
    }

    public void LoadPlugins(ICommand pluginCommand)
    {
        if (pluginCommand == null)
        {
            throw new ArgumentNullException(nameof(pluginCommand));
        }

        var path = Path.Combine(Environment.CurrentDirectory, "Plugins");
        var files = Directory.GetFiles(path, "*.plugin", SearchOption.AllDirectories)
                             .Select(x => Path.ChangeExtension(x, ".dll"));

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

        var finder =
            Codefarts.TypeLocator.TypeLocator.FindPublicClassesThatAreAssagnableTo<IPlugin<Application>>(files, contextCallback);

        // create types
        foreach (var item in finder)
        {
            if (pluginCommand.CanExecute(item))
            {
                //  var plugin = this.diProvider.Resolve(item) as IPlugin<Application>;
                pluginCommand.Execute(item);
            }
        }
    }
}
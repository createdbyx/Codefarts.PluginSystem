using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Loader;

namespace Codefarts.PluginSystem;

public class PluginSystem
{
    private Dictionary<string, PluginEntry> pluginContexts;
    public event Func<AssemblyLoadContext, AssemblyName, Assembly>? Resolving;

    public IReadOnlyDictionary<string, PluginEntry> PluginContexts
    {
        get
        {
            return new ReadOnlyDictionary<string, PluginEntry>(this.pluginContexts);
        }
    }

    public PluginSystem()
    {
        this.pluginContexts = new Dictionary<string, PluginEntry>();
    }

    public void UnloadPlugin<T>(T plugin)
    {
        // find the entry that matched the plugin
        var entry = this.pluginContexts.FirstOrDefault(x => x.Value.PluginReference.Equals(plugin));

        // check if entry was found
        if (entry.Value == null)
        {
            return;
        }

        // remove the entry from the dictionary
        this.pluginContexts.Remove(entry.Key);

        // unload the context
        entry.Value.Context.Unload();
    }

    private IEnumerable<(string TypeName, string AssemblyFile, string FullAssemblyFilePath)> BuildPluginInfo(string f)
    {
        var reader = new PluginReader();
        var pluginInfos = reader.Read(f);
        return pluginInfos.Select(y =>
        {
            var fullAssemblyFilePath = Path.IsPathRooted(y.AssemblyFile)
                ? y.AssemblyFile
                : Path.Combine(Path.GetDirectoryName(f), y.AssemblyFile);
            return (TypeName: y.TypeName, AssemblyFile: y.AssemblyFile, FullAssemblyFilePath: fullAssemblyFilePath);
        });
    }

    public void LoadPlugins<T>(IEnumerable<string> pluginFiles, Func<Type, T> creationCallback)
    {
        var foundTypes = pluginFiles.SelectMany(this.BuildPluginInfo).ToArray();

        // create types
        foreach (var item in foundTypes)
        {
            // generate a load context name by taking the full file path and appending the plugin name to the end
            var loadContextName = $"{item.TypeName} ==> {item.AssemblyFile}";

            // create a new assembly load context for the plugin
            var context = this.CreateAsslemblyLoadContextForFile(loadContextName);

            // load the assembly file into the context
            var assembly = context.LoadFromAssemblyPath(item.FullAssemblyFilePath);

            // get the type from the assembly
            var type = assembly.GetType(item.TypeName);

            var plugin = creationCallback(type);

            PluginEntry entry = new PluginEntry()
            {
                TypeName = item.TypeName,
                AssemblyFile = item.FullAssemblyFilePath,
                Context = context,
                PluginReference = plugin
            };

            this.pluginContexts.Add(loadContextName, entry);
        }
    }

    private AssemblyLoadContext CreateAsslemblyLoadContextForFile(string name)
    {
        // create a new assembly load context for the file
        var context = new AssemblyLoadContext(name, true);

        // hook into the resolving event so that we can load any dependencies
        context.Resolving += this.Resolving ?? this.OnContextResolving;

        return context;
    }

    private Assembly OnContextResolving(AssemblyLoadContext context, AssemblyName name)
    {
        var assembly = context.LoadFromAssemblyName(name);
        return assembly;
    }
}
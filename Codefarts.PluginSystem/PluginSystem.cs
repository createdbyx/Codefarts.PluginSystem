using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Loader;

namespace Codefarts.PluginSystem;

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
        this.pluginContexts = new Dictionary<string, AssemblyLoadContext>();
    }

    public void UnloadPlugin<T>(T plugin)
    {
        // remove the plugin from the plugins collection
        this.app.Plugins.Remove(plugin);

        // call disconnect
        plugin.Disconnect();

        // find the assembly load context for the plugin but exclude the Default context
        var context = AssemblyLoadContext.All.FirstOrDefault(x => x.Assemblies.Any(asm => asm.Equals(plugin.GetType().Assembly)));

        // check how many other plugins are using the same context
        var count = this.app.Plugins.Count(x => x.GetType().Assembly.Equals(context.Assemblies.FirstOrDefault()));

        // if there are no other plugins using the same context then unload it
        if (count == 0)
        {
            context.Unload();
        }
    }

    public void LoadPlugins<T>(IEnumerable<string> assemblyFiles)
    {
        // build plugin folder path
//        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");


        // find all plugin assemblies by there association with a .plugin file
        // var files = searchFolders.SelectMany(sf=> Directory.GetFiles(sf, "*.plugin", SearchOption.AllDirectories)
        //                      .Select(x => Path.ChangeExtension(x, ".dll")).ToList();
        // files.AddRange(AppDomain.CurrentDomain.GetAssemblies().Select(x => x.Location));

        // // find all plugin assemblies by there association with a .plugin file
        // var files = Directory.GetFiles(path, "*.plugin", SearchOption.AllDirectories)
        //                      .Select(x => Path.ChangeExtension(x, ".dll")).ToList();
        // files.AddRange(AppDomain.CurrentDomain.GetAssemblies().Select(x => x.Location));


        var foundTypes = TypeLocator.TypeLocator.FindPublicClassesThatAreAssagnableTo<T>(assemblyFiles);

        // create types
        foreach (var item in foundTypes)
        {
            // generate a load context name by taking the full file path and appending the plugin name to the end
            var loadContextName = $"{item.Type} ==> {item.File}";

            // create a new assembly load context for the plugin
            var context = this.CreateAsslemblyLoadContextForFile(loadContextName);

            // load the assembly file into the context
            var assembly = context.LoadFromAssemblyPath(item.File);
                         
            // get the type from the assembly
            var type = assembly.GetType(item.Type);

            // create the plugin using the dependency injection provider
            var plugin = this.diProvider.Resolve(type) as T;

            // add the plugin to the plugins collection
            this.app.Plugins.Add(plugin);

            // call connect on the plugin
            plugin.Connect(this.app);
        }
    }

    private MetadataLoadContext CreateAsslemblyMetadataLoadContextForFile(string file)
    {
        // Create PathAssemblyResolver that can resolve assemblies using the created list.
        var resolver = new PathAssemblyResolver(new[] { file });

        // create a new metadata assembly load context for the file
        return new MetadataLoadContext(resolver);
    }

    private AssemblyLoadContext CreateAsslemblyLoadContextForFile(string name)
    {
        // create a new assembly load context for the file
        var context = new AssemblyLoadContext(name, true);

        // hook into the resolving event so that we can load any dependencies
        context.Resolving += this.OnContextResolving;

        // add the context to the collection
        this.pluginContexts.Add(name, context);

        return context;
    }

    private Assembly OnContextResolving(AssemblyLoadContext context, AssemblyName name)
    {
        var assembly = context.LoadFromAssemblyName(name);
        return assembly;
    }
}
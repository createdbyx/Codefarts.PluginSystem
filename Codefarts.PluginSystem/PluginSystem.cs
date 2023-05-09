using System.Collections.ObjectModel;
using System.Net.Mime;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;

namespace Codefarts.PluginSystem;

public class PluginEntry
{
    public string TypeName { get; set; }
    public string AssemblyFile { get; set; }
    public AssemblyLoadContext Context { get; set; }
    public object PluginReference { get; set; }
}

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

        // // find the assembly load context for the plugin but exclude the Default context
        // var context = AssemblyLoadContext.All.FirstOrDefault(x => x.Assemblies.Any(asm => asm.Equals(plugin.GetType().Assembly)));
        //                  AssemblyLoadContext.Default.
        // // check how many other plugins are using the same context
        // var count = this.app.Plugins.Count(x => x.GetType().Assembly.Equals(context.Assemblies.FirstOrDefault()));
        //
        // // if there are no other plugins using the same context then unload it
        // if (count == 0)
        // {
        //     context.Unload();
        // }
    }

    public void LoadPlugins<T>(IEnumerable<string> pluginFiles, Func<Type, T> instanciationCallback)
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


        // var foundTypes = TypeLocator.TypeLocator.FindPublicClassesThatAreAssagnableTo<T>(assemblyFiles);
        var reader = new PluginReader();
        var foundTypes = pluginFiles.Where(x => Path.GetExtension(x).Equals(".plugin", StringComparison.InvariantCultureIgnoreCase))
                                    .SelectMany(f =>
                                    {
                                        var pluginInfos = reader.Read(f);

                                        return pluginInfos.Select(y =>
                                                                      (TypeName: y.TypeName,
                                                                          AssemblyFile: y.AssemblyFile,
                                                                          FullAssemblyFilePath: Path.IsPathRooted(y.AssemblyFile)
                                                                              ? y.AssemblyFile
                                                                              : Path.Combine(Path.GetDirectoryName(f), y.AssemblyFile)
                                                                      )
                                        );
                                    }).ToArray();


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

            var plugin = instanciationCallback(type);

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

    // private MetadataLoadContext CreateAsslemblyMetadataLoadContextForFile(string file)
    // {
    //     // Create PathAssemblyResolver that can resolve assemblies using the created list.
    //     var resolver = new PathAssemblyResolver(new[] { file });
    //
    //     // create a new metadata assembly load context for the file
    //     return new MetadataLoadContext(resolver);
    // }

    private AssemblyLoadContext CreateAsslemblyLoadContextForFile(string name)
    {
        // create a new assembly load context for the file
        var context = new AssemblyLoadContext(name, true);

        // hook into the resolving event so that we can load any dependencies
        context.Resolving += this.Resolving ?? this.OnContextResolving;

        // add the context to the collection
        //  this.pluginContexts.Add(name, context);

        return context;
    }

    private Assembly OnContextResolving(AssemblyLoadContext context, AssemblyName name)
    {
        var assembly = context.LoadFromAssemblyName(name);
        return assembly;
    }
}
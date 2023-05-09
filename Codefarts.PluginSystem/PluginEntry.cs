using System.Runtime.Loader;

namespace Codefarts.PluginSystem;

public class PluginEntry
{
    public string TypeName { get; set; }
    public string AssemblyFile { get; set; }
    public AssemblyLoadContext Context { get; set; }
    public object PluginReference { get; set; }
}
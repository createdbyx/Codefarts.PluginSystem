using System.Xml.Linq;

namespace Codefarts.PluginSystem;

public class PluginInfo
{
    public string TypeName { get; set; }
    public string AssemblyFile { get; set; }
}

public class PluginReader  
{
    public PluginReader()
    {
    }

    public IEnumerable<PluginInfo> Read(string file)
    {
        var doc = XDocument.Load(file);
        var root = doc.Root;
        return ParseDoc(root);
    }

    public IEnumerable<PluginInfo> Read(Stream stream)
    {
        var doc = XDocument.Load(stream);
        var root = doc.Root;
        return ParseDoc(root);
    }

    private IEnumerable<PluginInfo> ParseDoc(XElement? root)
    {
        if (root == null)
        {
            yield break;
        }

        var plugins = root.Element("Plugins");
        if (plugins == null)
        {
            yield break;
        }

        foreach (var plugin in plugins.Elements("Plugin"))
        {
            var typeName = plugin.Attribute("class")?.Value;
            var assemblyFile = plugin.Attribute("assembly")?.Value;
            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(assemblyFile))
            {
                continue;
            }

            yield return new PluginInfo
            {
                TypeName = typeName,
                AssemblyFile = assemblyFile
            };
        }
    }
}
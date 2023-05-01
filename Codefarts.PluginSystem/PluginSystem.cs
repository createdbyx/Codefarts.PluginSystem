namespace Codefarts.PluginSystem;

public interface IPlugin<T> where T : class
{
    void Connect(T model);
    void Disconnect ();
}

public class PluginSystem
{
    
}
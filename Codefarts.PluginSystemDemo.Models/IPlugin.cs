namespace Codefarts.PluginSystemDemo.Models;

public interface IPlugin<T> where T : class
{
    void Connect(T model);
    void Disconnect ();
}
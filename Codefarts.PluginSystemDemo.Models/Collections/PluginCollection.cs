using System.Collections.ObjectModel;
using Codefarts.PluginSystem;

namespace Codefarts.PluginSystemDemo.Models.Collections;

public class PluginCollection : ObservableCollection<IPlugin<Application>>
{
}
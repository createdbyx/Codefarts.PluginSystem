using System.Collections.ObjectModel;

namespace Codefarts.PluginSystemDemo.Models.Collections;

public class PluginCollection : ObservableCollection<IPlugin<Application>>
{
}
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Codefarts.PluginSystemDemo.Models.Collections;

public class MenuItemCollection : ObservableCollection<MenuItem>
{
    public MenuItemCollection()
    {
    }

    public MenuItemCollection(IEnumerable<MenuItem> collection) : base(collection)
    {
    }

    public MenuItemCollection(List<MenuItem> list) : base(list)
    {
    }
}
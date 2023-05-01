using System;
using System.Diagnostics;
using System.Windows.Input;
using Codefarts.PluginSystemDemo.Models.Collections;

namespace Codefarts.PluginSystemDemo.Models;

public class MenuItem : BaseClass
{
    public event EventHandler Selected;
    
    private string title;
    // private ICommand command;
    // private object commandParameter;
    //
    // public object CommandParameter
    // {
    //     get
    //     {
    //         return this.commandParameter;
    //     }
    //
    //     set
    //     {
    //         this.SetField(ref this.commandParameter, value);
    //     }
    // }
    //
    // public ICommand Command
    // {
    //     get
    //     {
    //         return this.command;
    //     }
    //
    //     set
    //     {
    //         this.SetField(ref this.command, value);
    //     }
    // }

    public MenuItem()
    {
        this.subMenus = new MenuItemCollection();
    }

    private MenuItemCollection subMenus;

    public MenuItemCollection SubMenus
    {
        get
        {
            return this.subMenus;
        }

        set
        {
            this.SetField(ref this.subMenus, value);
        }
    }

    public string Title                     
    {
        get
        {
            return this.title;
        }

        set
        {
            this.SetField(ref this.title, value);
        }
    }

    public virtual void Select()
    {
        // get the invocation list and raise each delegate individually
        var invocationList = this.Selected?.GetInvocationList();
        if (invocationList == null)
        {
            return;
        }

        foreach (var invocation in invocationList)
        {
            try
            {
                invocation.DynamicInvoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
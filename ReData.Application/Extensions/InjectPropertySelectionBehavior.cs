using System.Reflection;
using System.Runtime.CompilerServices;
using SimpleInjector.Advanced;

// ReSharper disable once CheckNamespace
namespace ReData.Application;

class InjectPropertySelectionBehavior : IPropertySelectionBehavior
{
    public bool SelectProperty(Type implementationType, PropertyInfo prop)
    {
        // свойство должно быть недоступно публично
        if (prop.GetMethod?.IsPublic == true)
        {
            return false;
        }

        // свойство должно быть requiered
        if (prop.CustomAttributes.All(a => a.AttributeType != typeof(RequiredMemberAttribute)))
        {
            return false;
        }

        if (prop.SetMethod?.IsPublic == true)
        {
            return true;
        }

        return false;
    }
}
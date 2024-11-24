using System.Reflection;
using System.Runtime.CompilerServices;
using SimpleInjector.Advanced;

// ReSharper disable once CheckNamespace
namespace ReData.Application;

sealed class InjectPropertySelectionBehavior : IPropertySelectionBehavior
{
    public bool SelectProperty(Type implementationType, PropertyInfo propertyInfo)
    {
        // свойство должно быть недоступно публично
        if (propertyInfo.GetMethod?.IsPublic == true)
        {
            return false;
        }

        // свойство должно быть requiered
        if (propertyInfo.CustomAttributes.All(a => a.AttributeType != typeof(RequiredMemberAttribute)))
        {
            return false;
        }

        if (propertyInfo.SetMethod?.IsPublic == true)
        {
            return true;
        }

        return false;
    }
}
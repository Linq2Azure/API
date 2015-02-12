using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Linq2Azure.VirtualMachines;

namespace Linq2Azure
{
    static class XmlHelper
    {
        /// <summary>
        /// Populates an object from an XElement, based on properties whose names match XML elements.
        /// This handles properties with string, int and enum types and ignores all other properties.
        /// </summary>
        public static void HydrateObject(this XElement element, XNamespace ns, object target)
        {

            if (element == null) // base case
                return;


            foreach (var prop in target.GetType().GetProperties())
            {
                var child = element.Element(ns + prop.Name);
                if (child == null)
                {
                    continue;
                }

                object value = null;
                if (Attribute.IsDefined(prop, typeof (TraverseAttribute)))
                {

                 //   Debug.WriteLine(prop.Name + " Traverse");

                    var instance = Activator.CreateInstance(prop.PropertyType);

                    if (prop.PropertyType.IsGenericType)
                    {

                      //  Debug.WriteLine(prop.Name + " Generic");

                        var genericArguments = prop.PropertyType.GetGenericArguments();
                        if (typeof(ICollection<>).MakeGenericType(genericArguments).IsAssignableFrom(prop.PropertyType))
                        {

                           // Debug.WriteLine(prop.Name + " Collection");

                            var subProps = child.Elements();

                           // Debug.WriteLine(subProps.Count() + " Count Sub Props");

                            foreach (var item in subProps)
                            {
                                var obj = Activator.CreateInstance(genericArguments.First());
                                HydrateObject(item, ns, obj);
                                instance.GetType().GetMethod("Add").Invoke(instance, new object[]{obj});
                            }
                        }

                    }
                    
               

                    HydrateObject(child,ns,instance);
                    prop.SetValue(target, instance);
                    continue;
                }else if (Attribute.IsDefined(prop, typeof (IgnoreAttribute)))
                {
                   // Debug.WriteLine(prop.Name + " Ignored");
                    continue;
                    
                }
                else if  (prop.PropertyType == typeof(string))
                    value = child.Value;
                else if (prop.PropertyType == typeof(Uri))
                    value = string.IsNullOrWhiteSpace(child.Value) ? null : new Uri(child.Value);
                else if (prop.PropertyType == typeof(int))
                    value = (int)child;
                else if (prop.PropertyType == typeof(int?))
                    value = string.IsNullOrWhiteSpace(child.Value) ? (int?)null : (int)child;
                else if (prop.PropertyType == typeof(long))
                    value = (long)child;
                else if (prop.PropertyType == typeof(long?))
                    value = string.IsNullOrWhiteSpace(child.Value) ? (long?)null : (long)child;
                else if (prop.PropertyType == typeof(decimal?))
                    value = string.IsNullOrWhiteSpace(child.Value) ? (decimal?)null : (decimal)child;
                else if (prop.PropertyType == typeof(decimal))
                    value = (decimal)child;
                else if (prop.PropertyType == typeof(bool))
                    value = (bool)child;
                else if (prop.PropertyType == typeof(Guid))
                    value = new Guid(child.Value);
                else if (prop.PropertyType == typeof(DateTimeOffset))
                    value = DateTimeOffset.Parse(child.Value);
                else if (prop.PropertyType == typeof(DateTimeOffset?))
                {
                    DateTimeOffset temp;
                    value = DateTimeOffset.TryParse(child.Value, out temp)
                        ? temp
                        : (DateTimeOffset?)null;
                }
                else if (prop.PropertyType.IsEnum)
                    value = Enum.Parse(prop.PropertyType, child.Value, true);
                else
                    continue;

                prop.SetValue(target, value);

            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Xml.Linq;

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
            foreach (var prop in target.GetType().GetProperties())
            {
                var child = element.Element(ns + prop.Name);
                if (child == null)
                {
                    continue;
                }

                object value;
                if (prop.PropertyType == typeof(string))
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

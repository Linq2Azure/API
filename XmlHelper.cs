using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
                if (child != null)
                {
                    object value;
                    
                    if (prop.PropertyType == typeof(string))
                        value = child.Value;
                    else if (prop.PropertyType == typeof(int))
                        value = (int)child;
                    else if (prop.PropertyType.IsEnum)
                        value = Enum.Parse(prop.PropertyType, child.Value, true);
                    else
                        continue;

                    prop.SetValue(target, value);
                }
            }
        }
    }
}

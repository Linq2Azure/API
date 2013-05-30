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
        public static void HydrateObject(this XElement element, XNamespace ns, object target)
        {
            foreach (var prop in target.GetType().GetProperties().Where(p => p.PropertyType == typeof(string)))
            {                
                var child = element.Element(ns + prop.Name);
                if (child != null) prop.SetValue(target, child.Value);
            }
        }
    }
}

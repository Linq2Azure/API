using LINQPad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Linq2Azure.LINQPadDriver
{
    /// <summary>
    /// This tells LINQPad not to render properties that we don't want to see when dumping.
    /// </summary>
    class CustomMemberProvider : LINQPad.ICustomMemberProvider
    {
        public static bool IsInteresting(Type t)
        {
            if (t.IsEnum || !t.Namespace.StartsWith (SchemaBuilder.Linq2AzureNamespace)) return false;
            if (t.GetProperty ("Subscription") == null && t.GetProperty ("Parent") == null) return false;
            return true;
        }

        object _objectToWrite;
        PropertyInfo[] _propsToWrite;

        public CustomMemberProvider(object objectToWrite)
        {
            _objectToWrite = objectToWrite;
            _propsToWrite = objectToWrite.GetType().GetProperties()
              .Where(p => p.GetIndexParameters().Length == 0 && p.Name != "Subscription" && p.Name != "Parent")
              .ToArray();
        }

        public IEnumerable<string> GetNames()
        {
            return _propsToWrite.Select(p => p.Name);
        }

        public IEnumerable<Type> GetTypes()
        {
            return _propsToWrite.Select(p => p.PropertyType);
        }

        public IEnumerable<object> GetValues()
        {
            return _propsToWrite.Select(p => p.GetValue(_objectToWrite, null));
        }
    }

}

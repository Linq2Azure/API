using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Linq2Azure.LINQPadDriver
{
    static class SchemaBuilder
    {
        internal static List<ExplorerItem> GetSchemaAndBuildAssembly(Linq2AzureProperties props, AssemblyName name,
            ref string nameSpace, ref string typeName)
        {
            string dllName = Path.GetFileName(name.CodeBase);
            string simpleName = Path.GetFileNameWithoutExtension(dllName);
            string folder = Path.GetDirectoryName(name.CodeBase);

            AppDomain appDomain = AppDomain.CurrentDomain;
            AssemblyBuilder assemBuilder = appDomain.DefineDynamicAssembly(new AssemblyName (simpleName), AssemblyBuilderAccess.RunAndSave, folder);
            ModuleBuilder modBuilder = assemBuilder.DefineDynamicModule(dllName);
            TypeBuilder tb = modBuilder.DefineType(nameSpace + "." + typeName, TypeAttributes.Public, typeof (Subscription));
            ConstructorBuilder c = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof (string) });
            var gen = c.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            ConstructorInfo baseConstr = typeof(Subscription).GetConstructor(new Type[] { typeof (string) });
            gen.Emit(OpCodes.Call, baseConstr);
            gen.Emit(OpCodes.Ret);

            tb.CreateType();
            assemBuilder.Save(dllName);

            return GetSchema(typeof(Subscription), 0);
        }

        internal static List<ExplorerItem> GetSchema(Type t, int level)
        {
            return t.GetProperties()
                .Where(p => p.Name != "Parent" && p.Name != "Subscription")
                .Select(p => ToExplorerItem(p, level))
                .OrderBy(p => GetIconDisplayOrder (p.Icon))
                .ThenBy(p => p.Text)
                .ToList();
        }

        static ExplorerItem ToExplorerItem(PropertyInfo p, int level)
        {
            Type t = p.PropertyType;
            if (t.IsEnum || !t.Namespace.StartsWith("Linq2Azure") || typeof(IFormattable).IsAssignableFrom(t))
                return new ExplorerItem(p.Name, ExplorerItemKind.Property, ExplorerIcon.Column) 
                {
                    ToolTipText = GetTypeName(t),
                    DragText = p.Name
                };

            Type elementType = GetLatentSequenceElementType(t);
            bool isLatentSequence = elementType != null;
            if (elementType == null) elementType = GetEnumerableElementType(t);
            bool isSequence = elementType != null;

            var item = new ExplorerItem(
                p.Name,
                ExplorerItemKind.QueryableObject,
                elementType == null ? ExplorerIcon.OneToOne : level == 0 ? ExplorerIcon.Table : ExplorerIcon.OneToMany)
            {
                ToolTipText = GetTypeName(t),
                IsEnumerable = isSequence
            };

            item.DragText = item.Text + (isLatentSequence ? ".AsObservable()" : "");

            if (level < 10)
                item.Children = GetSchema(elementType ?? t, level + 1);

            return item;
        }

        static string GetTypeName(Type t, int level = 0)
        {
            if (level > 3) return "";

            if (t.IsArray) 
                return GetTypeName (t.GetElementType(), level + 1) + "[".PadRight(t.GetArrayRank(), ',') + "]";

            if (t.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                return GetTypeName(t.GetGenericArguments()[0], level + 1) + "?";

            if (t.IsGenericType)
                return t.Name.Split ('`').First() +
                    "<" + 
                    string.Join(",", t.GetGenericArguments().Select(a => GetTypeName (a, level + 1))) +
                    ">";

            return t.Name;
        }

        static int GetIconDisplayOrder(ExplorerIcon icon)
        {
            switch (icon)
            {
                case ExplorerIcon.Column: return 0;
                case ExplorerIcon.OneToOne: return 1;
                case ExplorerIcon.OneToMany: return 2;
                case ExplorerIcon.Table: return 3;
                default: return 1;
            }
        }

        static Type GetLatentSequenceElementType(Type t)
        {
            if (t.IsInterface) return null;
            while (t != null && t != typeof(object))
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(LatentSequence<>)) return t.GetGenericArguments()[0];
                t = t.BaseType;
            }
            return null;
        }

        static Type GetEnumerableElementType(Type t)
        {
            var ie = t.GetInterface("System.Collections.Generic.IEnumerable`1");
            if (ie == null) return null;
            return ie.GetGenericArguments()[0];
        }        
    }
}

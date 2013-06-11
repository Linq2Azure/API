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
        public static readonly string Linq2AzureNamespace = typeof(Subscription).Namespace;

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
            var propItems = t.GetProperties()
                .Where(p => p.Name != "Parent" && p.Name != "Subscription")
                .Select(p => ToExplorerItem(p, level));

            var methodItems = t.GetMethods()
                .Where(m => m.ReturnType != null && typeof(Task).IsAssignableFrom (m.ReturnType))
                .Select(m => ToExplorerItem(m, level));

            return propItems.Concat (methodItems)
                .OrderBy(ei => GetIconDisplayOrder(ei.Icon)).ThenBy(ei => ei.Text)
                .ToList();
        }

        static ExplorerItem ToExplorerItem(PropertyInfo p, int level)
        {
            Type t = p.PropertyType;

            if (t.IsEnum || t.IsPrimitive || typeof(IFormattable).IsAssignableFrom(t))
                return ToSimpleExplorerItem(p);

            Type elementType = GetLatentSequenceElementType(t);
            bool isLatentSequence = elementType != null;
            if (elementType == null) elementType = GetEnumerableElementType(t);

            if (elementType != null && !elementType.Namespace.StartsWith(Linq2AzureNamespace) ||
                elementType == null && !t.Namespace.StartsWith(Linq2AzureNamespace))
                return ToSimpleExplorerItem(p);

            var item = new ExplorerItem(
                p.Name,
                ExplorerItemKind.QueryableObject,
                elementType == null ? ExplorerIcon.OneToOne : level == 0 ? ExplorerIcon.Table : ExplorerIcon.OneToMany)
            {
                ToolTipText = FormatTypeName(t),
                IsEnumerable = elementType != null && level == 0
            };

            item.DragText = item.Text + (isLatentSequence ? ".AsObservable()" : "");

            if (level < 10)
                item.Children = GetSchema(elementType ?? t, level + 1);

            return item;
        }

        static ExplorerItem ToSimpleExplorerItem(PropertyInfo p)
        {
            return new ExplorerItem(p.Name, ExplorerItemKind.Property, ExplorerIcon.Column)
            {
                ToolTipText = FormatTypeName(p.PropertyType),
                DragText = p.Name
            };
        }

        static ExplorerItem ToExplorerItem(MethodInfo m, int level)
        {
            var item = new ExplorerItem(m.Name, ExplorerItemKind.Property, ExplorerIcon.ScalarFunction)
            {
                ToolTipText = FormatTypeName(m.ReturnType),
                DragText = m.Name + "()"
            };

            item.Children = m.GetParameters()
                .Select(p => new ExplorerItem(FormatTypeName (p.ParameterType) + " " + p.Name, ExplorerItemKind.Parameter, ExplorerIcon.Parameter))
                .ToList();

            return item;
        }

        static string FormatTypeName(Type t, int level = 0)
        {
            if (level > 3) return "";

            if (t.IsArray) 
                return FormatTypeName (t.GetElementType(), level + 1) + "[".PadRight(t.GetArrayRank(), ',') + "]";

            if (t.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                return FormatTypeName(t.GetGenericArguments()[0], level + 1) + "?";

            if (t.IsGenericType)
                return t.Name.Split ('`').First() +
                    "<" + 
                    string.Join(",", t.GetGenericArguments().Select(a => FormatTypeName (a, level + 1))) +
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
                case ExplorerIcon.ScalarFunction: return 4;
                default: return 10;
            }
        }

        public static Type GetLatentSequenceElementType(Type t)
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

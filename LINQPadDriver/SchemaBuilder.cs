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
    class SchemaBuilder
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

            return new List<ExplorerItem>();
        }
    }
}

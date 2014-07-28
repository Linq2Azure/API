using System.Net;
using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linq2Azure.LINQPadDriver
{
    public class Linq2AzureDriver : DynamicDataContextDriver
    {
        public override string Name { get { return "Linq2Azure Azure Management API"; } }

        public override string Author { get { return "Cash Converters"; } }

        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return new Linq2AzureProperties(cxInfo).PublishSettingsPath;
        }

        public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
        {
            // We need the following assembly for compiliation and autocompletion:
            return new[] { "System.Runtime.dll", "Linq2Azure.dll", "System.Reactive.Core.dll", "System.Reactive.Interfaces.dll", "System.Reactive.Linq.dll" };
        }

        public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
        {
            // Import the commonly used namespaces as a courtesy to the user:
            return new[]
            {
                "Linq2Azure",
                "Linq2Azure.CloudServices",
                "Linq2Azure.SqlDatabases",
                "Linq2Azure.TrafficManagement",
                "Linq2Azure.StorageAccounts",
				"System.Reactive",
				"System.Reactive.Linq",
                "System.Reactive.Joins",
                "System.Reactive.Concurrency",
                "System.Reactive.Disposables",
                "System.Reactive.Subjects",
                "System.Reactive.Threading.Tasks"
            };
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            return new[] { new ParameterDescriptor("publishSettingsPath", "System.String") };
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            return new object[] { new Linq2AzureProperties(cxInfo).PublishSettingsPath };
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            ServicePointManager.DefaultConnectionLimit = new Linq2AzureProperties(cxInfo).ConnectionLimit;
        }

        public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
        {
            var p1 = new Linq2AzureProperties(c1);
            var p2 = new Linq2AzureProperties(c2);
            return string.Equals(p1.PublishSettingsPath, p2.PublishSettingsPath, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            return new ConnectionDialog(cxInfo).ShowDialog() == true;
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, System.Reflection.AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            return SchemaBuilder.GetSchemaAndBuildAssembly(new Linq2AzureProperties(cxInfo), assemblyToBuild, ref nameSpace, ref typeName);
        }

        public override LINQPad.ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {
            if (objectToWrite != null && CustomMemberProvider.IsInteresting(objectToWrite.GetType()))
                return new CustomMemberProvider(objectToWrite);

            return null;
        }
    }
}

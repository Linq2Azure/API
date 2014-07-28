using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.StorageAccounts
{
    public class StorageAccountKey
    {
        public StorageAccountKey(KeyType keyType, string key, StorageAccount storageAccount)
        {
            Key = key;
            StorageAccount = storageAccount;
            KeyType = keyType;
        }

        public KeyType KeyType { get; private set; }
        public string Key { get; private set; }
        public StorageAccount StorageAccount { get; private set; }

        public async Task RegenerateKey()
        {
            var client = StorageAccount.GetRestClient("/keys?action=regenerate");

            var ns = XmlNamespaces.WindowsAzure;

            var content = new XElement(ns + "RegenerateKeys",
                new XElement(ns + "KeyType", KeyType.ToString()));

            await client.PostAsync(content);
        }
    }
}
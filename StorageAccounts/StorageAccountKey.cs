namespace Linq2Azure.StorageAccounts
{
    public class StorageAccountKey
    {
        public StorageAccountKey(KeyType keyType, string key)
        {
            Key = key;
            KeyType = keyType;
        }

        public KeyType KeyType { get; private set; }
        public string Key { get; private set; }
    }
}
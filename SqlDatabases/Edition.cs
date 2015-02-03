namespace Linq2Azure.SqlDatabases
{
    public class Edition
    {

        private Edition(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public static Edition Web = new Edition("Web");
        public static Edition Basic = new Edition("Basic");
        public static Edition Standard = new Edition("Standard");
        public static Edition Premium = new Edition("Premium");
        public static Edition Business = new Edition("Business");
    }
}
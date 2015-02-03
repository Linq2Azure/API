using System;

namespace Linq2Azure.SqlDatabases
{
    public class PerformanceLevel
    {

        public Guid Value { get; private set; }

        private PerformanceLevel(Guid value)
        {
            Value = value;
        }

        public static PerformanceLevel Basic = new PerformanceLevel(new Guid("dd6d99bb-f193-4ec1-86f2-43d3bccbc49c"));
        public static PerformanceLevel StandardS0 = new PerformanceLevel(new Guid("f1173c43-91bd-4aaa-973c-54e79e15235b"));
        public static PerformanceLevel StandardS1 = new PerformanceLevel(new Guid("1b1ebd4d-d903-4baa-97f9-4ea675f5e928"));
        public static PerformanceLevel StandardS2 = new PerformanceLevel(new Guid("455330e1-00cd-488b-b5fa-177c226f28b7"));
        public static PerformanceLevel StandardS3 = new PerformanceLevel(new Guid("789681b8-ca10-4eb0-bdf2-e0b050601b40"));
        public static PerformanceLevel PremiumS1 = new PerformanceLevel(new Guid("7203483a-c4fb-4304-9e9f-17c71c904f5d"));
        public static PerformanceLevel PremiumS2 = new PerformanceLevel(new Guid("a7d1b92d-c987-4375-b54d-2b1d0e0f5bb0"));
        public static PerformanceLevel PremiumS3 = new PerformanceLevel(new Guid("a7c4c615-cfb1-464b-b252-925be0a19446"));
    }
}
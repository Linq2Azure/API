namespace Linq2Azure.VirtualMachines
{
    public class DiskLabel
    {
        protected DiskLabel(string label)
        {
            Label = label;
        }

        public static DiskLabel Is(string label)
        {
            return new DiskLabel(label);
        }

        public string Label { get; private set; }
    }
}
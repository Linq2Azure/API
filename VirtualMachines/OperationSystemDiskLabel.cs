namespace Linq2Azure.VirtualMachines
{
    public class OperationSystemDiskLabel
    {
        protected OperationSystemDiskLabel(string label)
        {
            Label = label;
        }

        public static OperationSystemDiskLabel Is(string label)
        {
            return new OperationSystemDiskLabel(label);
        }

        public string Label { get; private set; }
    }
}
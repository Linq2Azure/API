namespace Linq2Azure.VirtualMachines
{
    public class ImageName
    {
        protected ImageName(string name)
        {
            OsName = name;
        }

        public static ImageName Named(string name)
        {
            return new ImageName(name);
        }

        public string OsName { get; private set; }
    }
}
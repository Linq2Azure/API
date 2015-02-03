using System;

namespace Linq2Azure
{
    public static class Scoped
    {
        public static void Execute(Action f, Action t)
        {
            try
            {
                f();
            }
            finally
            {
                t();
            }
        }
    }
}
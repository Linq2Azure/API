using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IntegrationTests
{
    internal static class TaskExtensions
    {
        public static Task Catch<T>(this Task<T> t)
        {
            return Task.Run( async () =>
            {
                try
                {
                    await t;
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e);
                }
            });
        }
    }
}
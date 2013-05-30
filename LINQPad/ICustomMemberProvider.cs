using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LINQPad
{
    public interface ICustomMemberProvider
    {
        // Each of these methods must return a sequence with the same number of elements:
        IEnumerable<string> GetNames();
        IEnumerable<Type> GetTypes();
        IEnumerable<object> GetValues();
    }	
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;

namespace Linq2Azure
{
    public class LatentSequence<T> 
    {
        readonly Func<Task<T[]>> _taskGenerator;

        public LatentSequence(Func<Task<T[]>> taskGenerator)
        {
            _taskGenerator = taskGenerator;
        }

        public Task<T[]> AsTask() { return _taskGenerator(); }

        public T[] AsArray() { return AsTask().Result; }

        public IEnumerable<T> AsEnumerable()
        {
            foreach (var item in AsArray()) yield return item;
        }

        public IObservable<T> AsObservable()
        {
            return Observable.Defer(() => AsTask().ToObservable()).SelectMany(x => x);
        }      
    }
}

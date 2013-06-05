using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;

namespace Linq2Azure
{
    /// <summary>
    /// Represents a sequence whose elements become available some time in the future. This is used for collections
    /// that require a request over the Internet to populate.
    /// </summary>
    public class LatentSequence<T>
    {
        readonly Func<Task<T[]>> _taskGenerator;

        public LatentSequence(Func<Task<T[]>> taskGenerator)
        {
            _taskGenerator = taskGenerator;
        }

        /// <summary>Starts fetching data. The task completes when all elements have been fetched.</summary>
        public Task<T[]> AsTask() { return _taskGenerator(); }

        /// <summary>Fetches all elements, blocking until all data is available. Equivalent to calling AsTask().Result.</summary>
        public T[] AsArray() { return AsTask().Result; }

        /// <summary>Fetches all elements upon enumeration, blocking until all data is available. Equivalent to calling AsArray()
        /// except that the request is not kicked off until the sequence is actually enumerated.</summary>
        public IEnumerable<T> AsEnumerable()
        {
            return AsArray().Select(e => e);
        }

        /// <summary>Upon subscription, starts fetching data and then yields all elements when they become available. This 
        /// is useful in parallelizing queries with Reactive Extensions.</summary>
        public IObservable<T> AsObservable()
        {
            return Observable.Defer(() => AsTask().ToObservable()).SelectMany(x => x);
        }
    }
}

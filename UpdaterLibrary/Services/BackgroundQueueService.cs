using System;
using System.Threading;
using System.Threading.Tasks;
using UpdaterLibrary.Interfaces;

namespace UpdaterLibrary.Services
{
    public class BackgroundQueueService : IBackgroundQueueService
    {
        private Task _previousTask = Task.FromResult(true);
        private readonly object _key = new object();

        public Task<T> QueueTask<T>(Func<T> work)
        {
            lock (_key)
            {
                var task = _previousTask.ContinueWith(t => work()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                _previousTask = task;
                return task;
            }
        }
    }
}

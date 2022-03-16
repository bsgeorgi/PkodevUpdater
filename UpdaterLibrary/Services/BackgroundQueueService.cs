using System;
using System.Threading;
using System.Threading.Tasks;
using UpdaterLibrary.Interfaces;

namespace UpdaterLibrary.Services
{
    public class BackgroundQueueService : IBackgroundQueueService
    {
        private Task _previousTask = Task.FromResult(true);
        private object key = new object();

        public Task QueueTask(Action action)
        {
            lock (key)
            {
                _previousTask = _previousTask.ContinueWith(t => action()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                return _previousTask;
            }
        }

        public Task<T> QueueTask<T>(Func<T> work)
        {
            lock (key)
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

using System;
using System.Threading.Tasks;

namespace UpdaterLibrary.Interfaces
{
    public interface IBackgroundQueueService
    {
        public Task QueueTask(Action action);

        public Task<T> QueueTask<T>(Func<T> work);
    }
}

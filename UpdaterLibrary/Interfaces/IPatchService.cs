using System.Collections.Generic;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.Interfaces
{
    public interface IPatchService
    {
        public Queue<CommitFile> GetUpdateQueue();
    }
}

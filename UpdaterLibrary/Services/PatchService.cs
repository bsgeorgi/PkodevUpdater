using System.Collections.Generic;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.Services
{
    public class PatchService : IPatchService
    {
        public PatchService()
        {

        }

        public Queue<CommitFile> GetUpdateQueue()
        {
            // TODO: Retrieve a queue of files that need to be
            // Updated based on its status (modified, added, deleted)

            // To do that we need to get the current client commit hash
            // And compare it against the latest available in the repository
            // Then we get a list of files in between those commits
            // And append them to this queue
            return new Queue<CommitFile>();
        }
    }
}

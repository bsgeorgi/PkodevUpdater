using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace PkodevUpdater.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UpdaterWindow : Window, INotifyPropertyChanged
    {
        private readonly ICommitService _commitService;
        private readonly IPatchService _patchService;
        private readonly IBackgroundQueueService _backgroundQueueService;
        private readonly IOptions<AppSettings> _appSettings;
        public bool IsGameUpToDate { get; set; }

        public UpdaterWindow(ICommitService commitService, IPatchService patchService,
            IBackgroundQueueService backgroundQueueService, IOptions<AppSettings> appSettings)
        {
            DataContext = this;
            InitializeComponent();

            _commitService = commitService;
            _patchService = patchService;
            _backgroundQueueService = backgroundQueueService;
            _appSettings = appSettings;
        }

        /// <summary>
        /// Ensures that the window is draggable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        /// <summary>
        /// Handler for IsGameUpToDate.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
        
        /// <summary>
        /// Handler for IsGameUpToDate.
        /// </summary>
        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Shuts down the application on close button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Event that is triggered once the application main form has been initialised.
        /// Triggers the methods to update the game client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FrameworkElement_OnInitialized(object? sender, EventArgs e)
        {
            // Update title to the one from settings
            Title = _appSettings.Value.Title;

            // Check if client is already up to date
            var isClientUpToDate = await _patchService.IsClientUpToDateAsync();
            if (isClientUpToDate)
            {
                // Change IsGameUpToDate to true which will
                // Trigger the start game button styles and enable it
                IsGameUpToDate = true;
                OnPropertyChanged("IsGameUpToDate");

                // Reset the progress bar to 100 value and change
                // Its intermediate property to false
                await ResetProgressBar(100);
                await UpdateControlThreadSafe(ProgressLabel, "Content",
                    "Game is up to date. Click Start Game to start playing!");
            }
            else
            {
                // Retrieve the queue of updates to install them
                await UpdateControlThreadSafe(ProgressLabel, "Content",
                    "Receiving update information...");
                var updateQueue = await _patchService.GetUpdateQueueAsync();

                await ResetProgressBar();
                await UpdateControlThreadSafe(ProgressLabel, "Content",
                    "Preparing to install updates...");

                // Processes the queue of files whether they have been
                // Modified or deleted
                await InstallUpdatesAsync(updateQueue);
            }
        }

        /// <summary>
        /// Processes a queue of updates. Downloads and deletes files if necessary.
        /// </summary>
        /// <param name="updateQueue"></param>
        /// <returns></returns>
        private async Task InstallUpdatesAsync(Queue<CommitFile> updateQueue)
        {
            if (updateQueue.Count > 0)
            {
                foreach (var commit in updateQueue)
                {
                    switch (commit.Status)
                    {
                        case "modified":
                        case "added":
                            _backgroundQueueService.QueueTask(() => DownloadFile(commit)).Wait();
                            break;
                        case "removed":
                            _backgroundQueueService.QueueTask(() => DeleteFile(commit.Name)).Wait();
                            break;
                    }
                }

                updateQueue.Clear();
            }

            // If the queue was empty for some reason
            // Then we assume that client is up to date
            // And overwrite the current version
            // With the hash of the latest commit in the repository
            await UpdateControlThreadSafe(ProgressLabel, "Content",
                "Receiving latest client version...");
            var lastCommit = await _commitService.GetLastCommitAsync();
            
            await UpdateControlThreadSafe(ProgressLabel, "Content",
                "Updating game client version...");
            var updateClient = _patchService.SetClientVersion(lastCommit.Sha);

            if (updateClient)
            {
                await ResetProgressBar(100);
                await UpdateControlThreadSafe(ProgressLabel, "Content",
                    "Game is up to date. Click Start Game to start playing!");

                IsGameUpToDate = true;
                OnPropertyChanged("IsGameUpToDate");
            }
            else
            {
                // TODO: perhaps it would be a good idea to log all those errors
                await UpdateControlThreadSafe(ProgressLabel, "Content",
                    "An error occurred while updating version!");
            }
        }

        private async Task DeleteFile(string file)
        {
            await UpdateControlThreadSafe(ProgressLabel, "Content",
                $"Removing file {file}");
            _patchService.TryDeleteFile(file);
        }

        private async Task DownloadFile(CommitFile commitFile)
        {
            await UpdateControlThreadSafe(ProgressLabel, "Content",
                $"Updating file {commitFile.Name}");
            _patchService.DownloadFile(commitFile.Url, commitFile.Name);
        }

        private static async Task UpdateControlThreadSafe(Control control, string propertyName,
            object propertyValue)
        {
            await Task.Factory.StartNew(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    control.GetType().InvokeMember(
                        propertyName,
                        BindingFlags.SetProperty,
                        null,
                        control,
                        new[] { propertyValue });
                });
            });
        }

        private async Task ResetProgressBar(int value = 0)
        {
            // Update controls outside of main thread to not
            // Block the GUI
            await UpdateControlThreadSafe(PkodevProgressBar, "IsIndeterminate", false);
            await UpdateControlThreadSafe(PkodevProgressBar, "Value", value);
        }
    }
}

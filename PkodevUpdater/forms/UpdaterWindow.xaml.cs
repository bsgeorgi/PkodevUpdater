using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UpdaterLibrary.Interfaces;
using UpdaterLibrary.Models;

namespace PkodevUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UpdaterWindow : Window, INotifyPropertyChanged
    {
        private readonly ICommitService _commitService;
        private readonly IPatchService _patchService;
        private readonly IBackgroundQueueService _backgroundQueueService;
        public bool IsGameUpToDate { get; set; }

        public UpdaterWindow(ICommitService commitService, IPatchService patchService,
            IBackgroundQueueService backgroundQueueService)
        {
            DataContext = this;
            InitializeComponent();

            _commitService = commitService;
            _patchService = patchService;
            _backgroundQueueService = backgroundQueueService;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow the window to be draggable
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void CloseBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private async void FrameworkElement_OnInitialized(object? sender, EventArgs e)
        {
            var isClientUpToDate = await _patchService.IsClientUpToDateAsync();
            if (isClientUpToDate)
            {
                IsGameUpToDate = true;
                OnPropertyChanged("IsGameUpToDate");
                ResetProgressBar(100);
                UpdateProgressLabel("Game is up to date. Click Start Game to start playing!");
            }
            else
            {
                UpdateProgressLabel("Receiving update information...");
                var updateQueue = await _patchService.GetUpdateQueueAsync();

                ResetProgressBar();
                ProgressLabel.Content = "Preparing to install updates...";

                await InstallUpdatesAsync(updateQueue);
            }
        }

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
                            _backgroundQueueService.QueueTask(() => DownloadFile(commit.Name)).Wait();
                            break;
                        case "removed":
                            _backgroundQueueService.QueueTask(() => DeleteFile(commit.Name)).Wait();
                            break;
                    }
                }

                updateQueue.Clear();
            }
            
            UpdateProgressLabel("Receiving latest client version...");
            var lastCommit = await _commitService.GetLastCommitAsync();

            UpdateProgressLabel("Updating game client version...");
            var updateClient = _patchService.SetClientVersion(lastCommit.Sha);

            if (updateClient)
            {
                ResetProgressBar(100);
                UpdateProgressLabel("Game is up to date. Click Start Game to start playing!");

                IsGameUpToDate = true;
                OnPropertyChanged("IsGameUpToDate");
            }
            else
            {
                UpdateProgressLabel("An error occurred while updating version.");
            }
        }

        private void DeleteFile(string file)
        {
            UpdateProgressLabel("Deleting file " + file);
        }

        private void DownloadFile(string file)
        {
            UpdateProgressLabel("Downloading file " + file);
        }

        private async void ResetProgressBar(int value = 0)
        {
            await Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PkodevProgressBar.IsIndeterminate = false;
                    PkodevProgressBar.Value = value;
                });
            });
        }

        private async void UpdateProgressLabel(string content)
        {
            await Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ProgressLabel.Content = content;
                }));
            });
        }
    }
}

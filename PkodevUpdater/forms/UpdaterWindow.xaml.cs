using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UpdaterLibrary.Interfaces;

namespace PkodevUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UpdaterWindow : Window, INotifyPropertyChanged
    {
        private readonly ICommitService _commitService;
        private readonly IPatchService _patchService;
        public bool IsGameUpToDate { get; set; }

        public UpdaterWindow(ICommitService commitService, IPatchService patchService)
        {
            _commitService = commitService;
            _patchService = patchService;
            DataContext = this;
            InitializeComponent();

            IsGameUpToDate = true;
            OnPropertyChanged("IsGameUpToDate");
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
            var updates = await _patchService.GetUpdateQueueAsync();

            ResetProgressBar();
            ProgressLabel.Content = "Preparing to install updates...";

            //foreach (var commit in updates)
            //{
            //    MessageBox.Show($"{commit.Name} - {commit.Status} - {commit.Url}");
            //}
        }

        private async void ResetProgressBar()
        {
            await Task.Delay(100).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PkodevProgressBar.IsIndeterminate = false;
                    PkodevProgressBar.Value = 0;
                });
            });
        }
    }
}

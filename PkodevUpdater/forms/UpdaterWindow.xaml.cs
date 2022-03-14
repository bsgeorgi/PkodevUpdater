using System;
using System.ComponentModel;
using System.Linq;
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
        private readonly IRepositoryService _repositoryService;
        public bool IsGameUpToDate { get; set; }

        public UpdaterWindow(ICommitService commitService, IRepositoryService repositoryService)
        {
            DataContext = this;
            InitializeComponent();

            IsGameUpToDate = true;
            OnPropertyChanged("IsGameUpToDate");

            _commitService = commitService;
            _repositoryService = repositoryService;
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
            var commit = await _commitService.GetCommitInfoAsync("9a8944231ff5347d3879f7c61b0bec87d7c66d38")
                .ConfigureAwait(false);

            foreach (var file in commit.Files)
            {
                MessageBox.Show($"{file.Filename} = {file.Status}");
            }
        }
    }
}

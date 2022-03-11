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
        private readonly IGithubService _githubService;
        public bool IsGameUpToDate { get; set; }

        public UpdaterWindow(IGithubService githubService)
        {
            DataContext = this;
            InitializeComponent();

            IsGameUpToDate = true;
            OnPropertyChanged("IsGameUpToDate");

            _githubService = githubService;
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
            var commits = await _githubService.GetAllCommitsAsync();
            if (commits != null && commits.Any())
            {
                foreach (var commit in commits)
                {
                    MessageBox.Show(commit.Sha);
                }
            }
        }
    }
}

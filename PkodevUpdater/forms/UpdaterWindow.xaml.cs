using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Options;
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
        private readonly IRepositoryService _repositoryService;
        private readonly IVersionService _versionService;
        private readonly IOptions<AppSettings> _appSettings;
        public bool IsGameUpToDate { get; set; }

        public UpdaterWindow(ICommitService commitService, IRepositoryService repositoryService,
            IVersionService versionService, IOptions<AppSettings> appSettings)
        {
            DataContext = this;
            InitializeComponent();

            IsGameUpToDate = true;
            OnPropertyChanged("IsGameUpToDate");

            _commitService = commitService;
            _repositoryService = repositoryService;
            _versionService = versionService;
            _appSettings = appSettings;
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
            var gameVersion = _versionService.UpdateClientVersion("afas");
            MessageBox.Show(gameVersion.ToString());
        }
    }
}

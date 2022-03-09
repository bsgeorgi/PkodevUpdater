using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PkodevUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UpdaterWindow : Window, INotifyPropertyChanged
    {
        public bool IsGameUpToDate { get; set; }

        public UpdaterWindow()
        {
            DataContext = this;
            InitializeComponent();
            IsGameUpToDate = true;
            OnPropertyChanged("IsGameUpToDate");
        }

        private void Grid_Initialized(object sender, EventArgs e)
        {
            Title = ConfigurationManager.AppSettings["Title"] ?? "Game Updater";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow the window to be draggable
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        /*private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Flag = true;
            OnPropertyChanged("Flag");
        }*/

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void CloseBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}

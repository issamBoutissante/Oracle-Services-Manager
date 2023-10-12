using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceProcess;
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

namespace OracleManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.ServiceProcess;
    using System.Windows;

    public partial class MainWindow : Window
    {
        public ObservableCollection<OracleServiceInfo> Services { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Services = new ObservableCollection<OracleServiceInfo>();
            OracleServiceList.ItemsSource = Services;
            UpdateServiceStatus();
        }

        private void UpdateServiceStatus()
        {
            var systemServices = ServiceController.GetServices();
            var oracleServices = systemServices.Where(s => s.ServiceName.StartsWith("OracleService")).ToList();

            Services.Clear();

            foreach (var service in oracleServices)
            {
                Services.Add(new OracleServiceInfo
                {
                    Name = service.ServiceName,
                    CanStart = service.Status == ServiceControllerStatus.Stopped,
                    CanStop = service.Status == ServiceControllerStatus.Running,
                    IsProgressVisible = Visibility.Hidden
                });
            }
        }

        private async void ControlService(string serviceName, bool start)
        {
            var serviceInfo = Services.First(s => s.Name == serviceName);
            serviceInfo.IsProgressVisible = Visibility.Visible;

            await Task.Run(() =>
            {
                var service = new ServiceController(serviceName);
                if (start && service.Status == ServiceControllerStatus.Stopped)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
                else if (!start && service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            });

            UpdateServiceStatus();
            serviceInfo.IsProgressVisible = Visibility.Hidden;
        }


        private void StartService_Click(object sender, RoutedEventArgs e)
        {
            var serviceName = ((OracleServiceInfo)((FrameworkElement)sender).DataContext).Name;
            ControlService(serviceName, true);
        }

        private void StopService_Click(object sender, RoutedEventArgs e)
        {
            var serviceName = ((OracleServiceInfo)((FrameworkElement)sender).DataContext).Name;
            ControlService(serviceName, false);
        }
    }

    public class OracleServiceInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private bool _canStart;
        private bool _canStop;
        private Visibility _isProgressVisible;

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        public bool CanStart
        {
            get { return _canStart; }
            set { _canStart = value; OnPropertyChanged(); }
        }

        public bool CanStop
        {
            get { return _canStop; }
            set { _canStop = value; OnPropertyChanged(); }
        }

        public Visibility IsProgressVisible
        {
            get { return _isProgressVisible; }
            set { _isProgressVisible = value; OnPropertyChanged(); }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}



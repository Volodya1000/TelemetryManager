using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.Application.Services;
using TelemetryManager.AvaloniaApp.ViewModels;

namespace TelemetryManager.AvaloniaApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(Program.ServiceProvider.GetService<DeviceService>());
        }
    }
}
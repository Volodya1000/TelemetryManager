using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using TelemetryManager.AvaloniaApp.ViewModels;

namespace TelemetryManager.AvaloniaApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Program.ServiceProvider
                        .GetRequiredService<MainWindowViewModel>();

            if (DataContext is MainWindowViewModel vm)
            {
                vm.OwnerWindow = this;
            }
        }
    }
}
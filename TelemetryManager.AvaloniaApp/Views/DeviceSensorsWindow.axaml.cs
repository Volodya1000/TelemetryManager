using Avalonia.Controls;
using DialogHostAvalonia;
using System;
using TelemetryManager.AvaloniaApp.ViewModels;
using TelemetryManager.Core.Data.ValueObjects;
namespace TelemetryManager.AvaloniaApp.Views;

public partial class DeviceSensorsWindow : Window
{
    public DeviceSensorsWindow()
    {
        InitializeComponent();
    }

    private void DialogHost_OnDialogClosing(object sender, DialogClosingEventArgs e)
    {
        if (e.Parameter is EditIntervalViewModel vm)
        {
            // ��������� ���������� ���������
            if (vm.NewMin >= vm.NewMax)
            {
                e.Cancel();
                return;
            }

            // ������ ��������� Parameter, ���������� Session.UpdateParameter()
            if (e.Session is { } session)
            {
                session.UpdateParameter(new Interval(vm.NewMin, vm.NewMax));
            }
        }
    }
}
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using TelemetryManager.Application.Services;

namespace TelemetryManager.AvaloniaApp.ViewModels
{
    public class DeviceDisplayItem
    {
        public ushort DeviceId { get; set; }
        public string Name { get; set; } = "";
        public string ActivationTime { get; set; } = "";
    }

    public class MainWindowViewModel : ReactiveObject
    {
        private readonly DeviceService _deviceService;

        // 1) Сделали Devices полноценным свойством с уведомлением
        private ObservableCollection<DeviceDisplayItem> _devices = new();
        public ObservableCollection<DeviceDisplayItem> Devices
        {
            get => _devices;
            set => this.RaiseAndSetIfChanged(ref _devices, value);
        }

        private string _newDeviceName = "";
        public string NewDeviceName
        {
            get => _newDeviceName;
            set => this.RaiseAndSetIfChanged(ref _newDeviceName, value);
        }

        private ushort _newDeviceId;
        public ushort NewDeviceId
        {
            get => _newDeviceId;
            set => this.RaiseAndSetIfChanged(ref _newDeviceId, value);
        }

        public ReactiveCommand<Unit, Unit> LoadDevicesCommand { get; }
        public ReactiveCommand<Unit, Unit> AddDeviceCommand { get; }

        public MainWindowViewModel(DeviceService deviceService)
        {
            _deviceService = deviceService;

            LoadDevicesCommand = ReactiveCommand.CreateFromTask(
                LoadDevicesAsync,
                outputScheduler: RxApp.MainThreadScheduler
            );

            var canAdd = this.WhenAnyValue(
                x => x.NewDeviceName,
                x => x.NewDeviceId,
                (name, id) => !string.IsNullOrWhiteSpace(name) && id > 0
            );

            AddDeviceCommand = ReactiveCommand.CreateFromTask(
                AddDeviceAsync,
                canAdd,
                outputScheduler: RxApp.MainThreadScheduler
            );

            // Загрузим список при старте
            LoadDevicesCommand.Execute().Subscribe();
        }

        private async Task LoadDevicesAsync()
        {
            var all = await _deviceService.GetAllAsync();

            // 2) Формируем новую коллекцию и присваиваем
            var list = new ObservableCollection<DeviceDisplayItem>();
            foreach (var d in all)
            {
                list.Add(new DeviceDisplayItem
                {
                    DeviceId = d.DeviceId,
                    Name = d.Name.ToString(),
                    ActivationTime = d.ActivationTime?.ToString("dd.MM.yyyy HH:mm")
                                     ?? "Не активирован"
                });
            }
            Devices = list;
        }

        private async Task AddDeviceAsync()
        {
            await _deviceService.AddAsync(NewDeviceId, NewDeviceName);

            // Сброс полей формы
            NewDeviceId = 0;
            NewDeviceName = "";

            // Перезагрузим список
            await LoadDevicesAsync();
        }
    }
}

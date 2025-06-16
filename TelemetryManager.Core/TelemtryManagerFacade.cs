


using System.ComponentModel.DataAnnotations;
using TelemetryManager.Core.Data.Profiles;
using TelemetryManager.Core.EventArgs;
using TelemetryManager.Core.Interfaces;

namespace TelemetryManager.Core;

public class TelemtryManagerFacade
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IConfigurationValidator _configurationValidator;

    private CancellationTokenSource? _cts;




    public TelemtryManagerFacade(
        IConfigurationLoader configurationLoader,
        IConfigurationValidator configurationValidator)
    {
        _configurationLoader = configurationLoader;
        _configurationValidator = configurationValidator;
    }
    public void LoadConfiguration(string configFilePath)
    {
        List<DeviceProfile> deviceProfiles = _configurationLoader.Load(configFilePath);
        _configurationValidator.Validate(deviceProfiles);
    }


    public void StartStream(Stream input)
    {
        // Если предыдущий поток запущен, остановим его
        if (_cts != null)
        {
            StopStream();
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(() =>
        {
            try
            {
               // using var reader = new PacketStreamReader(input);
                while (!token.IsCancellationRequested)
                {
                    //var raw = reader.ReadNextPacket();
                    //if (raw == null)
                    //    continue; // шум или EOF

                    //if (!_parser.TryParse(raw, out var packet))
                    //{
                    //    OnPacketRejected("Parse error", raw.ToArray());
                    //    continue;
                    //}

                    //var data = SensorDataDecoder.Decode(packet);
                    //_storage.AddData(data);
                    //OnSensorDataReceived(data);

                    //if (_validator.IsAnomalous(data, out var anomalyInfo))
                    //{
                    //    _storage.AddAnomaly(anomalyInfo);
                    //    OnAnomalyDetected(anomalyInfo);
                    //}
                }
            }
            catch (Exception ex)
            {
               // _logger.Error("Stream processing failed", ex);
            }
        }, token);
    }

    public void StopStream()
    {
        if (_cts == null) return;
        _cts.Cancel();
        _cts = null;
    }




    public event EventHandler<SensorDataEventArgs>? SensorDataReceived;
    public event EventHandler<AnomalyEventArgs>? AnomalyDetected;
    public event EventHandler<PacketErrorEventArgs>? PacketRejected;



    //protected virtual void OnSensorDataReceived(SensorDataEventArgs e)
    //=> SensorDataReceived?.Invoke(this, e);


    //protected virtual void OnAnomalyDetected(AnomalyEventArgs e)
    //   => AnomalyDetected?.Invoke(this, e);

    //protected virtual void OnPacketRejected(string reason, byte[] rawData)
    //  => PacketRejected?.Invoke(this,
    //      new PacketErrorEventArgs(reason, rawData, DateTime.UtcNow));


}

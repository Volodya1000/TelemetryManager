using System.Collections.Generic;
using TelemetryManager.Application.Interfaces;
using TelemetryManager.Application.Logger;
using TelemetryManager.Core.Data;
using TelemetryManager.Core.Data.Profiles;

namespace TelemetryManager.Application.Services;

public class TelemtryService
{
    private readonly IConfigurationLoader _configurationLoader;
    private readonly IConfigurationValidator _configurationValidator;
    private readonly IPacketStreamParser _parser;
    private readonly IErrorLogger _errorLogger;

    //  private CancellationTokenSource? _cts;

    private List<DeviceProfile> deviceProfiles;

    private readonly List<TelemetryPacket> recivedPackets = new List<TelemetryPacket>();
    private readonly List<ParsingError> parsingErrors = new List<ParsingError>();


    public TelemtryService(
         IConfigurationLoader configurationLoader,
         IConfigurationValidator configurationValidator,
         IPacketStreamParser parser,
         IErrorLogger errorLogger)
    {
        _configurationLoader = configurationLoader;
        _configurationValidator = configurationValidator;
        _parser= parser;
        _errorLogger = errorLogger;
    }
    public void LoadConfiguration(string configFilePath)
    {
        deviceProfiles = _configurationLoader.Load(configFilePath);
        _configurationValidator.Validate(deviceProfiles);
    }

    public void ProcessTelemetryFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} does not exist.", filePath);


        using (var stream = File.OpenRead(filePath))
        {
            var packeges = _parser.Parse(stream);
            recivedPackets.AddRange(packeges.Packets);
            parsingErrors.AddRange(packeges.Errors);
        }
    }

    public List<DeviceProfile> GetDevicesProfiles() => deviceProfiles;

    public List<TelemetryPacket> GetRecivedPackets() => recivedPackets;

    public List<ParsingError> GetParsingErrors() => parsingErrors;


    //public void StartStream(Stream input)
    //{
    //    // Если предыдущий поток запущен, остановим его
    //    if (_cts != null)
    //    {
    //        StopStream();
    //    }

    //    _cts = new CancellationTokenSource();
    //    var token = _cts.Token;

    //    Task.Run(() =>
    //    {
    //        try
    //        {
    //            var reader = new PacketStreamParser(input);
    //            while (!token.IsCancellationRequested)
    //            {
    //                var raw = reader.ReadNextPacket();
    //                if (raw == null)
    //                    continue; // шум или EOF

    //                //if (!_parser.TryParse(raw, out var packet))
    //                //{
    //                //    OnPacketRejected("Parse error", raw.ToArray());
    //                //    continue;
    //                //}

    //                //var data = SensorDataDecoder.Decode(packet);
    //                //_storage.AddData(data);
    //                //OnSensorDataReceived(data);




    //                //if (_validator.IsAnomalous(data, out var anomalyInfo))
    //                //{
    //                //    _storage.AddAnomaly(anomalyInfo);
    //                //    OnAnomalyDetected(anomalyInfo);
    //                //}
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //           // _logger.Error("Stream processing failed", ex);
    //        }
    //    }, token);
    //}

    //public void StopStream()
    //{
    //    if (_cts == null) return;
    //    _cts.Cancel();
    //    _cts = null;
    //}




    //public event EventHandler<SensorDataEventArgs>? SensorDataReceived;
    //public event EventHandler<AnomalyEventArgs>? AnomalyDetected;



    //protected virtual void OnSensorDataReceived(SensorDataEventArgs e)
    //=> SensorDataReceived?.Invoke(this, e);


    //protected virtual void OnAnomalyDetected(AnomalyEventArgs e)
    //   => AnomalyDetected?.Invoke(this, e);

}

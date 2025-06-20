﻿using TelemetryManager.Core.Data;
using TelemetryManager.Core.Identifiers;

namespace TelemetryManager.Application.Interfaces;

public interface ITelemetryService
{
    int DeviceId { get; }

    void LoadConfiguartion(string configFilePath);


    /// <summary>
    /// Начинает асинхронную обработку входного потока данных
    /// </summary>
    /// <param name="input">Поток данных (Serial/Udp/FileStream и т.п.).</param>
    Task StartStreamAsync(Stream stream);

    /// <summary>
    /// Останавливает обработку потока, если она запущена.
    /// </summary>
    Task StopStreamAsync();


    void ProcessTelemetryFile(string filePath);

    /// <summary>
    /// Возвращает текущее состояние всех сенсоров.
    /// </summary>
    IReadOnlyList<SensorSnapshot> GetCurrentSensorValues();


    /// <summary>
    /// Возвращает информацию о некорректных пакетах
    /// </summary>
   // IReadOnlyList<AnomalyRecord> GetAnomalies();


    IReadOnlyList<SensorHistoryRecord> GetSendorHistory(SensorId sensorId);



    ///// <summary>
    ///// Возникает при получении валидных данных о сенсоре.
    ///// </summary>
    //event EventHandler<SensorDataEventArgs> SensorDataReceived;

    ///// <summary>
    ///// Возникает при возникновении аномалии (выхода парметра за допусккаемые границы).
    ///// </summary>
    //event EventHandler<ParameterOutOfRangeEventArgs> AnomalyDetected;
}


namespace TelemetryManager.Persistence.Entities;

internal abstract class DbEntity<TIdentifier>
{
    public TIdentifier Id { get; set; }
}

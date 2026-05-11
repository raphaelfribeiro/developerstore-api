namespace Ambev.DeveloperEvaluation.ORM.Services;

/// <summary>
/// Configuration model for the MongoDB domain event store.
/// Bound from the "MongoDB" section of appsettings / environment variables.
/// If ConnectionString is empty the event store is disabled and the application
/// falls back to Serilog-only publishing.
/// </summary>
public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDB";

    public string ConnectionString { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = "developer_evaluation";

    public string EventsCollectionName { get; set; } = "domain_events";
}

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;

public class DatabaseHealthCheck(IDbConnectionFactory connectionFactory, ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
    public const string Name = "Database";
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            _ = await connectionFactory.CreateConnectionAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            const string errorMessage = "Database is unhealthy";
            logger.LogError(e, errorMessage);
            return HealthCheckResult.Unhealthy(errorMessage, e);
        }
    }
}
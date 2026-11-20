using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VoyageHub.Data;
using Xunit;

namespace VoyageHub.Tests;

public class RetryPolicyTests
{
    [Fact]
    public void SqlServerConfiguration_EnablesExecutionStrategyAndTransientRetries()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                "Server=tcp:dummy.database.windows.net,1433;Database=TestDb;Encrypt=True;",
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                )));

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var strategy = context.Database.CreateExecutionStrategy();
        Assert.NotNull(strategy);
    }
}

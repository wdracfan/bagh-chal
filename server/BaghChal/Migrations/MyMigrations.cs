using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Migrations;

public static class MyMigrations
{
    public static void DoMigration(string connectionString, bool down)
    {
        using (var serviceProvider = CreateServices(connectionString))
        using (var scope = serviceProvider.CreateScope())
        {
            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            UpdateDatabase(scope.ServiceProvider, down);
        }
    }

    /// <summary>
    /// Configure the dependency injection services
    /// </summary>
    private static ServiceProvider CreateServices(string connectionString)
    {
        return new ServiceCollection()
            // Add common FluentMigrator services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                // Add Postgres support to FluentMigrator
                .AddPostgres()
                // Set the connection string
                .WithGlobalConnectionString(connectionString)
                // Define the assembly containing the migrations
                .ScanIn(typeof(InitialMigration).Assembly).For.Migrations())
            // Enable logging to console in the FluentMigrator way
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            // Build the service provider
            .BuildServiceProvider(false);
    }

    /// <summary>
    /// Update the database
    /// </summary>
    private static void UpdateDatabase(IServiceProvider serviceProvider, bool down)
    {
        // Instantiate the runner
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        // Execute the migrations
        if (down)
        {
            runner.MigrateDown(0);
        }
        else
        {
            runner.MigrateUp();
        }
    }
}
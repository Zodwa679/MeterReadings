using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace MeterReadings.IntegrationTests;

/// <summary>
/// Hosts the real API in memory against a throwaway SQL Server container. The API runs in
/// Development, so it applies the migration and the seed data on startup exactly as it does locally.
/// </summary>
public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public Task InitializeAsync() => _sql.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _sql.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = new SqlConnectionStringBuilder(_sql.GetConnectionString())
        {
            InitialCatalog = "MeterReadings"
        }.ConnectionString;

        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
    }
}

/// <summary>One container and one API host shared by every integration test class.</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}

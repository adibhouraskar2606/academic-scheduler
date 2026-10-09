using AcademicScheduler.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AcademicScheduler.Tests.Helpers;

public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public AcademicSchedulerDbContext Context { get; }

    private TestDatabase(
        SqliteConnection connection,
        AcademicSchedulerDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public static async Task<TestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        try
        {
            var options =
                new DbContextOptionsBuilder<AcademicSchedulerDbContext>()
                    .UseSqlite(connection)
                    .Options;

            var context = new AcademicSchedulerDbContext(options);

            try
            {
                await context.Database.EnsureCreatedAsync();
                return new TestDatabase(connection, context);
            }
            catch
            {
                await context.DisposeAsync();
                throw;
            }
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
using BaghChal.Contracts;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BaghChal.Repositories;

// ХОРОШО БЫ СЮДА ПРИКРУТИТЬ REDIS, ЧТОБЫ ХРАНИТЬ ДОСКУ В ВИДЕ JSONЧИКА
public class GameRepository : IGameRepository
{
    private readonly NpgsqlConnection _conn;
    private readonly string _connectionString;

    public GameRepository(IOptions<PostgresOptions> postgresOptions)
    {
        _connectionString = postgresOptions.Value.ConnectionString;
        _conn = new NpgsqlConnection(_connectionString);
    }

    public async Task ChangePiece(int row, int col, char piece)
    {
        await _conn.ExecuteAsync($@"");
    }

    public Task RemovePiece(int row, int col, char piece)
    {
        throw new NotImplementedException();
    }

    public Task AddPiece(int row, int col, char piece)
    {
        throw new NotImplementedException();
    }
}
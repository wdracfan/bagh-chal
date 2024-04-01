using BaghChal.Contracts;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BaghChal.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly NpgsqlConnection _conn;
    private readonly string _connectionString;

    public BoardRepository(IOptions<PostgresOptions> postgresOptions)
    {
        _connectionString = postgresOptions.Value.ConnectionString;
        _conn = new NpgsqlConnection(_connectionString);
        Migrations.MyMigrations.DoMigration(_connectionString, down: true);
        Migrations.MyMigrations.DoMigration(_connectionString, down: false);
    }
    
    public async Task<Guid> RegisterUser(string username)
    {
        var userId = Guid.NewGuid();
        await _conn.ExecuteAsync($@"
            INSERT INTO game.users
            VALUES ('{userId}', '{username}')
        ");
        return userId;
    }

    public async Task<Guid> CreateBoard(Guid userId, string piece)
    {
        var boardId = Guid.NewGuid();
        await _conn.ExecuteAsync($@"
            INSERT INTO game.boards
            VALUES ('{boardId}', '{userId}', null, '{piece}')
        ");
        return boardId;
    }

    public async Task<(Guid, string)> JoinBoard(Guid boardId, Guid userId)
    { 
        var hostId = (await _conn.QueryAsync<string>($@"
            SELECT host_guid FROM game.boards b
            WHERE b.board_guid = '{boardId}'
        ")).Select(x => new Guid(x)).First();
        var guestId = (await _conn.QueryAsync<string>($@"
            SELECT guest_guid FROM game.boards b
            WHERE b.board_guid = '{boardId}'
        ")).First();
        var piece = (await _conn.QueryAsync<string>($@"
            SELECT piece FROM game.boards b
            WHERE b.board_guid = '{boardId}'
        ")).First();
        Console.WriteLine(guestId);
        if (guestId is not null)
        {
            throw new NotSupportedException("к этой доске уже присоединился второй игрок!");
        }
        await _conn.ExecuteAsync($@"
            UPDATE game.boards SET guest_guid = '{userId}'
            WHERE board_guid = '{boardId}'
        ");
        // Console.WriteLine("WWWWW" + hostId);
        // логика + проверка на то, что к игре ещё никто не присоединился
        return (hostId, piece);
    }

    public async Task<string> FindName(Guid guid)
    {
        var name = (await _conn.QueryAsync<string>($@"
            SELECT username FROM game.users u
            WHERE u.user_guid = '{guid}'
        ")).First();
        if (name is null)
        {
            throw new Exception(); // подумать
        }
        return name;
    }
}
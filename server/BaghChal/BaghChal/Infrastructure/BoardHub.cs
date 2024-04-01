using BaghChal.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace BaghChal.Infrastructure;

public class BoardHub : Hub
{
    private readonly IGameRepository _gameRepository;
    
    //каждый раз создаётся заново экземпляр класса, поэтому нужно static
    private static readonly IDictionary<string, BoardDto> _dictionary = new Dictionary<string, BoardDto>();

    public BoardHub(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
        // _dictionary = new Dictionary<string, BoardDto>();
    }

    private void Initialize(string boardId)
    {
        if (_dictionary.ContainsKey(boardId) is false)
        {
            _dictionary[boardId] = new BoardDto(); // concurrency?
        }
    }

    private async Task<bool> CheckWinner(string boardId)
    {
        var winner = _dictionary[boardId].WhoWins();
        if (winner != 'O')
        {
            await Clients.Group(boardId).SendAsync(
                "check_winner_response", winner == 'G' ? "goat" : "tiger");
            return true;
        }

        return false;
    }
    
    public async Task PlaceNewGoat(string boardId, int row, int col)
    {
        Initialize(boardId); // нужно только при постановке первого козла
        await Clients.Group(boardId).SendAsync(
            "place_new_goat_response", row, col);

        _dictionary[boardId].Board[row][col] = 'G';
        _dictionary[boardId].SpareGoats -= 1;
        _dictionary[boardId].BoardGoats += 1;

        await CheckWinner(boardId);
    }

    public async Task MoveTiger(string boardId, int rowEx, int colEx, int rowNew, int colNew)
    {
        await Clients.Group(boardId).SendAsync(
            "move_tiger_response", rowEx, colEx, rowNew, colNew);
        
        _dictionary[boardId].Board[rowEx][colEx] = 'O';
        _dictionary[boardId].Board[rowNew][colNew] = 'T';
        
        // надо проверять только тогда, когда не съедена коза!!!
        await CheckWinner(boardId);
    }
    
    public async Task MoveGoat(string boardId, int rowEx, int colEx, int rowNew, int colNew)
    {
        await Clients.Group(boardId).SendAsync(
            "move_goat_response", rowEx, colEx, rowNew, colNew);
        
        _dictionary[boardId].Board[rowEx][colEx] = 'O';
        _dictionary[boardId].Board[rowNew][colNew] = 'G';
        
        await CheckWinner(boardId);
    }

    public async Task ChangeTurn(string boardId, bool withBot, string piece)
    {
        if (withBot is false)
        {
            await Clients.Group(boardId).SendAsync("change_turn_response");
        }
        else
        {
            if (piece == "goat") // игрок за goat, значит я за tiger
            {
                var move = _dictionary[boardId].GetRandomTigerMove();
                Console.WriteLine(move);
                if (move.Item1 == -27)
                {
                    await CheckWinner(boardId);
                }
                else
                {
                    if (move.Item5) // ест козу
                    {
                        await Clients.Group(boardId).SendAsync(
                            "move_tiger_response", 
                            move.Item1, 
                            move.Item2, 
                            2 * move.Item3 - move.Item1, 
                            2 * move.Item4 - move.Item2);
                        _dictionary[boardId].Board[move.Item1][move.Item2] = 'O';
                        _dictionary[boardId].Board[2 * move.Item3 - move.Item1][2 * move.Item4 - move.Item2] = 'T';
                        
                        await Clients.Group(boardId).SendAsync(
                            "take_goat_response", 
                            move.Item3, 
                            move.Item4);
                        _dictionary[boardId].Board[move.Item3][move.Item4] = 'O';
                        _dictionary[boardId].BoardGoats -= 1;
                    }
                    else // не ест козу
                    {
                        await Clients.Group(boardId).SendAsync(
                            "move_tiger_response", 
                            move.Item1, 
                            move.Item2, 
                            move.Item3, 
                            move.Item4);
                        _dictionary[boardId].Board[move.Item1][move.Item2] = 'O';
                        _dictionary[boardId].Board[move.Item3][move.Item4] = 'T';
                    }
                    
                    await CheckWinner(boardId);
                }
            }
            else
            {
                Initialize(boardId);
                var move = _dictionary[boardId].GetRandomGoatMove();
                Console.WriteLine(move);
                if (move.Item1 == -27)
                {
                    if (await CheckWinner(boardId))
                    {
                        return;
                    }
                    await Clients.Group(boardId).SendAsync(
                        "place_new_goat_response", move.Item3, move.Item4);

                    _dictionary[boardId].Board[move.Item3][move.Item4] = 'G';
                    _dictionary[boardId].SpareGoats -= 1;
                    _dictionary[boardId].BoardGoats += 1;
                }
                else
                {
                    await Clients.Group(boardId).SendAsync(
                        "move_goat_response", 
                        move.Item1, 
                        move.Item2, 
                        move.Item3,
                        move.Item4);
                    _dictionary[boardId].Board[move.Item1][move.Item2] = 'O';
                    _dictionary[boardId].Board[move.Item3][move.Item4] = 'G';
                }
                
                await CheckWinner(boardId);
            }
        }
    }

    public async Task TakeGoat(string boardId, int row, int col)
    {
        await Clients.Group(boardId).SendAsync(
            "take_goat_response", row, col);
        
        _dictionary[boardId].Board[row][col] = 'O';
        _dictionary[boardId].BoardGoats -= 1;
        
        await CheckWinner(boardId);
    }
}
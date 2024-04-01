namespace BaghChal.Infrastructure;

public class BoardDto
{
    public List<List<char>> Board { get; set; } = new ()
    {
        new List<char>(){'T', 'O', 'O', 'O', 'T'},
        new List<char>(){'O', 'O', 'O', 'O', 'O'},
        new List<char>(){'O', 'O', 'O', 'O', 'O'},
        new List<char>(){'O', 'O', 'O', 'O', 'O'},
        new List<char>(){'T', 'O', 'O', 'O', 'T'},
    };
    public int BoardGoats { get; set; } = 0;
    public int SpareGoats { get; set; } = 20;

    public char WhoWins()
    {
        if (BoardGoats == 0 && SpareGoats == 0)
        {
            return 'T';
        }

        var canTigersMove = false;
        for (var i = 0; i < 5; ++i)
        {
            for (var j = 0; j < 5; ++j)
            {
                if (Board[i][j] == 'T')
                {
                    canTigersMove = canTigersMove || CheckTigerMoves(i, j);
                }
            }
        }

        if (canTigersMove is false)
        {
            Console.WriteLine("GOATS WON");
            return 'G';
        }

        return 'O';
    }

    // проверяет, может ли тигр куда-нибудь пойти из своей клетки
    private bool CheckTigerMoves(int row, int col)
    {
        /*if ((row - col) % 2 == 0) // можно ходить во всех направлениях
        {
            if (IsValidTigerMove(row + 1, col + 1, row + 2, col + 2) ||
                IsValidTigerMove(row + 1, col - 1, row + 2, col - 2) ||
                IsValidTigerMove(row - 1, col - 1, row - 2, col - 2) ||
                IsValidTigerMove(row - 1, col + 1, row - 2, col + 2))
            {
                return true;
            }
        }

        return IsValidTigerMove(row + 1, col, row + 2, col) ||
               IsValidTigerMove(row, col + 1, row, col + 2) ||
               IsValidTigerMove(row - 1, col, row - 2, col) ||
               IsValidTigerMove(row, col - 1, row, col - 2);*/

        return GetRandomTigerMoveFromPoint(row, col).Count > 0;
    }

    public (int, int, int, int, bool) GetRandomTigerMove() // было x, было y, стало x, стало y, ест ли козу
    {
        var possibleMoves = new List<(int, int, int, int, bool)>();
        for (var i = 0; i < 5; ++i)
        {
            for (var j = 0; j < 5; ++j)
            {
                if (Board[i][j] == 'T')
                {
                    var move = GetRandomTigerMoveFromPoint(i, j);
                    possibleMoves.AddRange(move.Select(x => (i, j, x.Item1, x.Item2, x.Item3)));
                }
            }
        }

        if (possibleMoves.Count == 0)
        {
            return (-27, -27, -27, -27, false);
        }

        if (possibleMoves.Any(x => x.Item5))
        {
            var takeGoats = possibleMoves.Where(x => x.Item5).ToList();
            return takeGoats[new Random().Next(takeGoats.Count)];
        }
        return possibleMoves[new Random().Next(possibleMoves.Count)];
        // TODO: выбирать те ходы, на которых тигр ест козу!!!
    }
    
    // TODO: надо учитывать, что коз сначала надо расставить на доске
    public (int, int, int, int) GetRandomGoatMove()
    {
        var emptyPoints = new List<(int, int)>();
        var possibleMoves = new List<(int, int, int, int)>();
        for (var i = 0; i < 5; ++i)
        {
            for (var j = 0; j < 5; ++j)
            {
                if (Board[i][j] == 'G')
                {
                    var move = GetRandomGoatMoveFromPoint(i, j);
                    possibleMoves.AddRange(move.Select(x => (i, j, x.Item1, x.Item2)));
                }

                if (Board[i][j] == 'O')
                {
                    emptyPoints.Add((i, j));
                }
            }
        }

        if (SpareGoats > 0)
        {
            var res = emptyPoints[new Random().Next(emptyPoints.Count)];
            return (-27, -27, res.Item1, res.Item2);
        }
        if (possibleMoves.Count == 0)
        {
            return (-27, -27, -27, -27);
        }
        return possibleMoves[new Random().Next(possibleMoves.Count)];
    }

    private List<(int, int, bool)> GetRandomTigerMoveFromPoint(int row, int col) // (x, y, надо ли есть козу)
    {
        var result = new List<(int, int, bool)>();
        if ((row - col) % 2 == 0) // можно ходить во всех направлениях
        {
            if (IsValidTigerMove(row + 1, col + 1, row + 2, col + 2)) result.Add((row + 1, col + 1, Board[row + 1][col + 1] == 'G'));
            if (IsValidTigerMove(row + 1, col - 1, row + 2, col - 2)) result.Add((row + 1, col - 1, Board[row + 1][col - 1] == 'G'));
            if (IsValidTigerMove(row - 1, col - 1, row - 2, col - 2)) result.Add((row - 1, col - 1, Board[row - 1][col - 1] == 'G'));
            if (IsValidTigerMove(row - 1, col + 1, row - 2, col + 2)) result.Add((row - 1, col + 1, Board[row - 1][col + 1] == 'G'));
        }

        if (IsValidTigerMove(row + 1, col, row + 2, col)) result.Add((row + 1, col, Board[row + 1][col] == 'G'));
        if (IsValidTigerMove(row, col + 1, row, col + 2)) result.Add((row, col + 1, Board[row][col + 1] == 'G'));
        if (IsValidTigerMove(row - 1, col, row - 2, col)) result.Add((row - 1, col, Board[row - 1][col] == 'G'));
        if (IsValidTigerMove(row, col - 1, row, col - 2)) result.Add((row, col - 1, Board[row][col - 1] == 'G'));

        return result;
    }

    // ИСПРАВИТЬ по аналогии с предыдущим (делать список)
    private List<(int, int)> GetRandomGoatMoveFromPoint(int row, int col)
    {
        var result = new List<(int, int)>();
        if ((row - col) % 2 == 0) // можно ходить во всех направлениях
        {
            if (IsValidGoatMove(row + 1, col + 1)) result.Add((row + 1, col + 1));
            if (IsValidGoatMove(row + 1, col - 1)) result.Add((row + 1, col - 1));
            if (IsValidGoatMove(row - 1, col - 1)) result.Add((row - 1, col - 1));
            if (IsValidGoatMove(row - 1, col + 1)) result.Add((row - 1, col + 1));
        }

        if (IsValidGoatMove(row + 1, col)) result.Add((row + 1, col));
        if (IsValidGoatMove(row, col + 1)) result.Add((row, col + 1));
        if (IsValidGoatMove(row - 1, col)) result.Add((row - 1, col));
        if (IsValidGoatMove(row, col - 1)) result.Add((row, col - 1));

        return result;
    }

    private static bool IsOnField(int row, int col)
    {
        return row is >= 0 and < 5 && col is >= 0 and < 5;
    }

    private bool IsValidTigerMove(int row, int col, int overrow, int overcol) // over - клетка через одну, если придётся есть козу
    {
        if (IsOnField(row, col) is false)
        {
            //Console.WriteLine($"{row} {col} not on field");
            return false;
        }

        if (Board[row][col] == 'O')
        {
            //Console.WriteLine($"{row} {col} empty");
            return true;
        }

        return Board[row][col] == 'G' && IsOnField(overrow, overcol) && Board[overrow][overcol] == 'O';
    }

    private bool IsValidGoatMove(int row, int col)
    {
        return IsOnField(row, col) && Board[row][col] == 'O';
    }
}
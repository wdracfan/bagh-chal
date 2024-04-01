namespace BaghChal.Contracts;

public interface IGameRepository
{
    public Task ChangePiece(int row, int col, char piece);
    public Task RemovePiece(int row, int col, char piece);
    public Task AddPiece(int row, int col, char piece);
}
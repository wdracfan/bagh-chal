namespace BaghChal.Contracts;

public interface IBoardRepository
{
    public Task<Guid> RegisterUser(string username);
    public Task<Guid> CreateBoard(Guid userId, string piece);
    public Task<(Guid, string)> JoinBoard(Guid boardId, Guid userId);
    public Task<string> FindName(Guid guid);
}
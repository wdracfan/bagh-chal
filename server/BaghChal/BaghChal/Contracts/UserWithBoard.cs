namespace BaghChal.Contracts;

public class UserWithBoard
{
    public Guid UserId { get; set; }
    public Guid BoardId { get; set; }
    public string ConnectionId { get; set; } = "";
}
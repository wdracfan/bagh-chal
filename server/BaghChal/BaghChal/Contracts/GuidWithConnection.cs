namespace BaghChal.Contracts;

public class GuidWithConnection
{
    public Guid Guid { get; set; }
    public string ConnectionId { get; set; } = "";
    public string Piece { get; set; } = "";
}
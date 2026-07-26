namespace ThinQConnectApi.Models;

public abstract class BaseModel
{
    public required string MessageId { get; set; }

    public DateTime Timestamp { get; set; }
}
namespace Ices.Crossport.Core.Connecting;

public delegate void CellEventHandler(Cell sender);

/// <summary>
///     A broadcast domain with 1 Provider and N Consumers.
///     Notice: Cell WILL NOT manages lifetime of connection and provider.
/// </summary>
public class Cell
{
    public Cell(ContentProvider provider) { Provider = provider; }

    public Dictionary<ContentConsumer, NonPeerConnection> Consumers { get; } = new();

    public ContentProvider Provider { get; }

    public bool IsFull =>
        Provider.Capacity <= Consumers.Count;

    public bool IsAvailable
    {
        get
        {
            if (IsFull) return false;

            return Provider.Status is not (PeerStatus.Lost or PeerStatus.Dead);
        }
    }

    public event CellEventHandler? OnConnectionDestroyed;

    public async Task Connect(ContentConsumer consumer, NonPeerConnection connection)
    {
        connection.OnDestroyed += Connection_OnDestroyed;
        Consumers[consumer] = connection;
        await connection.SetProvider(Provider);
    }


    private async Task Connection_OnDestroyed(NonPeerConnection sender)
    {
        sender.OnDestroyed -= Connection_OnDestroyed;
        Consumers.Remove(sender.Consumer);
        OnConnectionDestroyed?.Invoke(this);
        await Task.CompletedTask;
    }
}
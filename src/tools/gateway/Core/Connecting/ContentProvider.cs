using Anonymous.Crossport.Core.Entities;
using Anonymous.Crossport.Core.Signalling;

namespace Anonymous.Crossport.Core.Connecting;

public class ContentProvider : Peer
{
    public ContentProvider(ISignalingHandler signaling, Guid id, CrossportConfig config, bool isCompatible) : base
    (
        signaling,
        id,
        config,
        isCompatible
    )
    {
        Capacity = config.Capacity;
    }

    public int Capacity { get; }
}
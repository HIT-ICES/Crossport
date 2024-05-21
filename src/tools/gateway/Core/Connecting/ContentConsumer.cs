using Anonymous.Crossport.Core.Entities;
using Anonymous.Crossport.Core.Signalling;

namespace Anonymous.Crossport.Core.Connecting;

public class ContentConsumer : Peer
{
    public ContentConsumer(ISignalingHandler signaling, Guid id, CrossportConfig config, bool isCompatible) : base
    (
        signaling,
        id,
        config,
        isCompatible
    ) { }
}
using Ices.Crossport.Core.Entities;
using Ices.Crossport.Core.Signalling;

namespace Ices.Crossport.Core.Connecting;

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
﻿namespace Anonymous.Crossport.Utils;

public static class CrossportEvents
{
    public static readonly EventId CrossportUndefinedException = new(650010, "Undefined Exception");
    public static readonly EventId CrossportDiagnosticDebugMessage = new(610020, "Debug Message");
    public static readonly EventId CrossportDiagnosticSignallingMessage = new(610021, "Debug Message");
    public static readonly EventId PeerCreated = new(620100, "Peer Created");
    public static readonly EventId PeerDead = new(630101, "Peer Dead");
    public static readonly EventId PeerReconnected = new(630102, "Peer Dead");
    public static readonly EventId PeerBadRegister = new(640103, "Peer Bad Register");
    public static readonly EventId CellCreated = new(620110, "Cell Created");

    public static readonly EventId NpcCreated =
        new(620200, "Non Peer Connection Created");

    public static readonly EventId NpcConsumerRequested =
        new(600201, "Non Peer Connection Consumer Requested (offer sent)");

    public static readonly EventId NpcProviderAnswered =
        new(610202, "Non Peer Connection Provider Answered (answer sent)");

    public static readonly EventId NpcProviderRequested =
        new(610203, "Non Peer Connection Provider Requested (offer sent)");

    public static readonly EventId NpcEstablished = new(620204, "Non Peer Connection Established");
    public static readonly EventId NpcDestroyed = new(630205, "Non Peer Connection Destroyed");
    public static readonly EventId NpcTimeout = new(640206, "Non Peer Connection Timeout");

    public static readonly EventId NpcProviderAlreadySet = new(640210, "Provider Already Set");
    public static readonly EventId NpcIllSigNullMessage = new(650211, "Message Parsing Failed");

    public static readonly EventId NpcIllSigConsumerOfferToNonPending =
        new(650212, "Consumer Offer To Non-Pending Connection");

    public static readonly EventId NpcIllSigConsumerAnswerToNonRequested =
        new(650213, "Consumer Answer To Non-Requested Connection");

    public static readonly EventId NpcIllSigConsumerMessageToNullProvider =
        new(650214, "Consumer Message To Null Provider");

    public static readonly EventId NpcIllSigProviderOfferToNonAnswered =
        new(650215, "Provider Offered before Answered");

    public static readonly EventId NpcIllSigProviderAnswerToNonRequested =
        new(650216, "Provider Answer To Non-Requested Connection");

    public static readonly EventId HealthListenerOnline =
        new(620300, "A Health Listener is now Online");

    public static readonly EventId EvaluationStarted =
        new(620400, "An Evaluation has been started.");

    public static readonly EventId EvaluationAlreadyRunning =
        new(630401, "An Evaluation failed to start because another one is running.");

    public static readonly EventId EvaluationStopped =
        new(620402, "An Evaluation has been stopped.");

    public static readonly EventId EvaluationStoppedWithoutResult =
        new(630403, "An Evaluation has been stopped, but no result was generated.");

    public static readonly EventId EvaluationStatsReceived =
        new(620410, "Received an Evaluation Stats from Client.");

    public static readonly EventId EvaluationStatsDropped =
        new(630411, "Dropped an Evaluation Stats from Client.");

    public static void LogCrossport(this ILogger logger, EventId eventId, string? message, params object?[] args)
    {
        logger.Log((LogLevel)((eventId.Id - 600000) / 10000), eventId, message, args);
    }
}
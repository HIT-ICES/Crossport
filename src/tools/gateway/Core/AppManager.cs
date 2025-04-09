using System.Collections.Concurrent;
using Ices.Crossport.Core.Connecting;
using Ices.Crossport.Core.Entities;
using Ices.Crossport.Core.Signalling;
using Ices.Crossport.Utils;
using Ices.MetaCdn.Health;
using Ices.MetaCdn.Remoting;

namespace Ices.Crossport.Core;

public record AppInfo(string Application, string Component)
{
    public AppInfo(CrossportConfig crossportConfig) : this(crossportConfig.Application, crossportConfig.Component) { }

    public AppInfo(AppComponentKey key) : this(key.App, key.Component) { }
}

public class AppManager
{
    private readonly RemotingMux _healthListeners = new();
    private readonly ILogger<AppManager> _logger;

    public AppManager(ILogger<AppManager> logger)
    {
        _logger = logger;
        _healthListeners.SubscribeOnMessage<AppComponentKey>
        (
            Enum.GetName(HealthEventType.Pull),
            (r, key) =>
            {
                if (AppComponents.TryGetValue(new AppInfo(key), out var app)) app.PullHealth(r);
            }
        );
    }

    public IEnumerable<AppInfo> AppInfos => AppComponents.Keys;
    private ConcurrentDictionary<AppInfo, AppComponent> AppComponents { get; } = new();
    private ConcurrentDictionary<Guid, Peer> Peers { get; } = new();

    public AppHealth? GetHealth(string application, string component)
    {
        return GetHealth(new AppInfo(application, component));
    }

    public AppHealth? GetHealth(AppInfo app)
    {
        return AppComponents.TryGetValue(app, out var health) ? health.GetHealth() : null;
    }

    public AppComponent EnsureAppComponent(AppInfo app)
    {
        return AppComponents.GetOrAdd
        (
            app,
            i =>
            {
                var app = new AppComponent(i, OnGeneralConnectionEvent);
                app.OnHealthChanged += OnAppHealthChanged;
                return app;
            }
        );
    }

    public async Task RegisterOrRenew
    (
        ISignalingHandler signaling,
        string connectionId,
        CrossportConfig? config,
        bool isCompatible
    )
    {
        if (!Guid.TryParse(connectionId, out var peerId))
            _logger.LogCrossport
            (
                CrossportEvents.PeerBadRegister,
                "Failed to parsing id for Peer {id}",
                connectionId
            );

        var peerType = isCompatible ? "Compatible" : "Standard";
        if (Peers.TryGetValue(peerId, out var peer))
        {
            peer.Reconnect(signaling, isCompatible);

            _logger.LogCrossport
            (
                CrossportEvents.PeerReconnected,
                "{app}/{comp}: {type} peer {id} reconnected successfully.",
                config?.Application,
                config?.Component,
                peerType,
                connectionId
            );

            return;
        }

        if (config is not null)
        {
            var app = EnsureAppComponent(new AppInfo(config));
            if (config.Capacity == 0)
            {
                var consumer = new ContentConsumer(signaling, peerId, config, isCompatible);
                app.Register(consumer);
                Peers[peerId] = consumer;
                _logger.LogCrossport
                (
                    CrossportEvents.PeerCreated,
                    "{app}/{comp}: {type} consumer {id} created successfully.",
                    config?.Application,
                    config?.Component,
                    peerType,
                    connectionId
                );
            }
            else
            {
                var provider = new ContentProvider(signaling, peerId, config, isCompatible);
                _logger.LogCrossport
                (
                    CrossportEvents.PeerCreated,
                    "{app}/{comp}: {type} provider peer {id} created successfully, capacity={cap}.",
                    config?.Application,
                    config?.Component,
                    peerType,
                    connectionId,
                    config?.Capacity
                );
                try
                {
                    var cell = await app.Register(provider);
                    Peers[peerId] = provider;
                    _logger.LogCrossport
                    (
                        CrossportEvents.CellCreated,
                        "Cell provided by peer {id} created successfully, {cnt} consumers are in.",
                        connectionId,
                        cell.Consumers.Count
                    );
                }
                catch (ProviderAlreadySetException e)
                {
                    _logger.LogCrossport
                    (
                        CrossportEvents.NpcProviderAlreadySet,
                        "Fatal: {eMessage} when setting provider",
                        e.Message
                    );
                }
            }
        }
        else
        {
            _logger.LogCrossport
            (
                CrossportEvents.PeerBadRegister,
                "Failed to parsing register data for Peer {id}",
                connectionId
            );
        }

        Peers[peerId].OnPeerDead += OnPeerDead;
    }

    private async void OnAppHealthChanged(AppComponent sender, HealthChange e)
    {
        await _healthListeners.SendAsync
        (
            Enum.GetName(HealthEventType.OnChangeOrPull),
            new AppHealth(sender.Info.Application, sender.Info.Component, e.New, e.Diff)
        );
    }

    public async Task ListenExceptions(Func<Task> run)
    {
        try
        {
            await run();
        }
        catch (ProviderAlreadySetException e)
        {
            _logger.LogCrossport
            (
                CrossportEvents.NpcProviderAlreadySet,
                "Fatal: {eMessage} when setting provider",
                e.Message
            );
        }
        catch (IllegalSignalingException e)
        {
            var eid = e.Type switch
                      {
                          IllegalSignalingException.IllegalSignalingType.NullMessage => CrossportEvents
                             .NpcIllSigNullMessage,
                          IllegalSignalingException.IllegalSignalingType.ConsumerOfferToNonPending => CrossportEvents
                             .NpcIllSigConsumerOfferToNonPending,
                          IllegalSignalingException.IllegalSignalingType.ConsumerAnswerToNonRequested => CrossportEvents
                             .NpcIllSigConsumerAnswerToNonRequested,
                          IllegalSignalingException.IllegalSignalingType.ConsumerMessageToNullProvider =>
                              CrossportEvents
                                 .NpcIllSigConsumerMessageToNullProvider,
                          IllegalSignalingException.IllegalSignalingType.ProviderOfferToNonAnswered => CrossportEvents
                             .NpcIllSigProviderOfferToNonAnswered,
                          IllegalSignalingException.IllegalSignalingType.ProviderAnswerToNonRequested => CrossportEvents
                             .NpcIllSigProviderAnswerToNonRequested,
                          _ => throw new ArgumentOutOfRangeException()
                      };
            _logger.LogCrossport
            (
                eid,
                "Npc {id} Illegal Signalling ({type}): {message}",
                e.Connection.Id,
                e.Type,
                e.SignallingData.ToString()
            );
        }
        catch (OperationCanceledException)
        {
            // Task Canceled
        }
        catch (Exception e)
        {
            _logger.LogError
            (
                CrossportEvents.CrossportUndefinedException,
                e,
                "Undefined Exception caught by AppManager Listener: {emessage}",
                e.Message
            );
        }
    }

    private Task OnGeneralConnectionEvent(NonPeerConnection connection, ConnectionEventType eventType)
    {
        return Task.Run
        (
            () =>
            {
                switch (eventType)
                {
                case ConnectionEventType.StateChanged:
                    _logger.LogCrossport
                    (
                        connection.State switch
                        {
                            ConnectionState.ConsumerRequested => CrossportEvents.NpcConsumerRequested,
                            ConnectionState.ProviderAnswered => CrossportEvents.NpcProviderAnswered,
                            ConnectionState.ProviderRequested => CrossportEvents.NpcProviderRequested,
                            ConnectionState.Established => CrossportEvents.NpcEstablished,
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        "Npc {id} has a new status {cstatus} now.",
                        connection.Id,
                        Enum.GetName(connection.State)
                    );
                    break;
                case ConnectionEventType.Timeout:
                    _logger.LogCrossport
                    (
                        CrossportEvents.NpcTimeout,
                        "Npc {id} stuck at status {cstatus} for over {ttl} ms, and is to be destroyed.",
                        connection.Id,
                        Enum.GetName(connection.State),
                        NonPeerConnection.OfferedConnectionLifetime
                    );
                    break;
                case ConnectionEventType.Destroyed:
                    _logger.LogCrossport
                    (
                        CrossportEvents.NpcDestroyed,
                        "Npc {id} is destroyed. Provider: {pstatus}. Consumer: {cstatus}",
                        connection.Id,
                        Enum.GetName(connection.Provider?.Status ?? PeerStatus.Raw),
                        Enum.GetName(connection.Consumer.Status)
                    );
                    break;
                case ConnectionEventType.Created:
                    _logger.LogCrossport(CrossportEvents.NpcCreated, "Npc {id} is created.", connection.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
        );
    }

    private void OnPeerDead(Peer obj)
    {
        _logger.LogCrossport
        (
            CrossportEvents.PeerDead,
            "Peer {id} is dead after {ttl} ms waiting for reconnection.",
            obj.Id,
            Peer.LostPeerLifetime
        );
    }

    public void AddHealthListener(IRemoting remoting)
    {
        _logger.LogCrossport(CrossportEvents.HealthListenerOnline, CrossportEvents.HealthListenerOnline.Name);
        _healthListeners.Add(remoting);
        foreach (var (_, app) in AppComponents) app.PullHealth(remoting);
    }
}
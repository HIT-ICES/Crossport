using System.Collections.Concurrent;
using Anonymous.Crossport.Core.Connecting;
using Anonymous.Crossport.Core.Entities;
using Anonymous.MetaCdn.Health;
using Anonymous.MetaCdn.Remoting;

namespace Anonymous.Crossport.Core;

public enum ConnectionEventType
{
    StateChanged = 1,
    Timeout = 2,
    Destroyed = 3,
    Created = 0
}

public delegate Task GeneralConnectionEventHandler(NonPeerConnection connection, ConnectionEventType eventType);

/// <summary>
///     Manage lifetime of Connection and Cell.
/// </summary>
public class AppComponent(AppInfo info, GeneralConnectionEventHandler connectionEventCallback)
{
    public delegate void AppComponentHealthChanged(AppComponent sender, HealthChange e);

    private readonly ConcurrentDictionary<ContentProvider, Cell> _cells = new();
    private readonly ConcurrentQueue<(ContentConsumer, NonPeerConnection)> _queuedConsumers = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private HealthStats _lastHealthStats;
    public AppConfig Config { get; set; } = AppConfig.Default;

    public AppInfo Info { get; } = info;

    private HealthStats ReportHealth()
    {
        var used = _cells.Sum(c => c.Value.Consumers.Count);
        var total = _cells.Sum(c => c.Key.Capacity);
        var gray = _cells.Select(c => c.Key)
                         .Where(p => p.Status == PeerStatus.Lost)
                         .Sum(p => p.Capacity);
        var pending = _queuedConsumers.Count;
        total -= gray;
        return new HealthStats(used, total, pending, gray);
    }

    public event AppComponentHealthChanged? OnHealthChanged;

    public AppHealth GetHealth()
    {
        return new AppHealth(Info.Application, Info.Component, _lastHealthStats, _lastHealthStats - _lastHealthStats);
    }

    public async void PullHealth(IRemoting remoting)
    {
        await remoting.SendAsync
        (
            Enum.GetName(HealthEventType.OnChangeOrPull),
            GetHealth()
        );
    }

    private async Task UpdateHealth()
    {
        var newHealth = ReportHealth();
        Console.WriteLine($"Health Updated: {newHealth}(Origin: {_lastHealthStats})");
        await _semaphore.WaitAsync();
        var diff = newHealth - _lastHealthStats;
        if (newHealth != _lastHealthStats)
            OnHealthChanged?.Invoke(this, new HealthChange(newHealth, diff));
        _lastHealthStats = newHealth;
        _semaphore.Release();
    }

    public void Register(ContentConsumer consumer)
    {
        consumer.OnConnect += Consumer_OnConnect;
        //consumer.OnPeerDead += Consumer_OnPeerDead;
    }

    private async Task Consumer_OnConnect(Peer sender, string connectionId)
    {
        if (sender is not ContentConsumer consumer)
            throw new ArgumentException("Only ContentConsumer is allowed to create connection.", nameof(sender));
        var availableCell = _cells.Values.FirstOrDefault(c => c.IsAvailable);
        var connection = new NonPeerConnection(Info, consumer, connectionId);
        connection.OnDestroyed += Connection_OnDestroyed;
        connection.OnTimeout += Connection_OnTimeout;
        connection.OnStateChanged += Connection_OnStateChanged;
        if (availableCell != null)
            await availableCell.Connect(consumer, connection);
        else
            _queuedConsumers.Enqueue((consumer, connection));
        await UpdateHealth();
        await connectionEventCallback(connection, ConnectionEventType.Created);
    }

    private Task Connection_OnStateChanged(NonPeerConnection sender)
    {
        return connectionEventCallback(sender, ConnectionEventType.StateChanged);
    }

    private async Task Connection_OnTimeout(NonPeerConnection sender)
    {
        await connectionEventCallback(sender, ConnectionEventType.Timeout);
        await sender.Destroy();
    }

    private async Task Connection_OnDestroyed(NonPeerConnection sender)
    {
        sender.OnDestroyed -= Connection_OnDestroyed;
        sender.OnTimeout -= Connection_OnTimeout;
        sender.OnStateChanged -= Connection_OnStateChanged;
        if (sender.Consumer.Status == PeerStatus.Dead) sender.Consumer.OnConnect -= Consumer_OnConnect;
        await connectionEventCallback(sender, ConnectionEventType.Destroyed);
    }

    public async Task<Cell> Register(ContentProvider provider)
    {
        var cell = new Cell(provider);
        provider.OnPeerDead += Provider_OnPeerDead;
        cell.OnConnectionDestroyed += Cell_OnConnectionDestroyed;
        while (cell.IsAvailable && _queuedConsumers.TryDequeue(out var tuple))
        {
            var (consumer, connection) = tuple;
            if (consumer.Status is not PeerStatus.Standard or PeerStatus.Compatible
             || connection.State == ConnectionState.Disconnected) continue;
            await cell.Connect(consumer, connection);
        }

        _cells[provider] = cell;
        await UpdateHealth();
        return cell;
    }

    private async void Provider_OnPeerDead(Peer obj)
    {
        if (obj is not ContentProvider provider) return;
        _ = _cells.TryRemove(provider, out _);
        await UpdateHealth();
    }

    private async void Cell_OnConnectionDestroyed(Cell sender) { await UpdateHealth(); }
}
using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Reoria.Engine.Network.Sockets;

public class ClientSocket : Socket
{
    protected virtual string DefaultAddress { get; init; }
    protected virtual int DefaultPort { get; init; }
    protected virtual int DefaultTimeout { get; init; }

    public ClientSocket(ILogger<ClientSocket> logger, IConfiguration configuration)
        : base(logger, configuration)
    {
        this.DefaultAddress = configuration["Networking:Address"] ?? "127.0.0.1";
        this.DefaultPort = Convert.ToInt32(configuration["Networking:Port"] ?? "7234");
        this.DefaultTimeout = Convert.ToInt32(configuration["Networking:Timeout"] ?? "5");
    }

    public override bool Connect(string address, int port, int timeout = 5)
        => this.ConnectAsync(address, port, timeout).GetAwaiter().GetResult();

    public virtual async Task<bool> ConnectAsync(string address, int port, int timeout = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            this.Logger.LogInformation("Attempting to connect to {Address}:{Port} with timeout {Timeout}s", address, port, timeout);

            NetPeer peer = this.Manager.Connect(address, port, this.ConnectionKey);

            Task<bool> connectionTask = Task.Run(async () =>
            {
                while (peer.ConnectionState is not ConnectionState.Connected and not ConnectionState.Disconnected)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.Logger.LogInformation("Connection attempt cancelled");
                        return false;
                    }
                    
                    await Task.Delay(100, cancellationToken);
                }
                return peer.ConnectionState == ConnectionState.Connected;
            }, cancellationToken);

            Task timeoutTask = Task.Delay(timeout * 1000, cancellationToken);

            Task completedTask = await Task.WhenAny(connectionTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                this.Logger.LogWarning("Connection to {Address}:{Port} timed out after {Timeout}s", address, port, timeout);
                this.Manager.DisconnectAll();
                return false;
            }

            bool connected = await connectionTask;
            
            if (connected)
            {
                this.Logger.LogInformation("Successfully connected to {Address}:{Port}", address, port);
            }
            else
            {
                this.Logger.LogWarning("Failed to connect to {Address}:{Port}", address, port);
            }

            return connected;
        }
        catch (OperationCanceledException)
        {
            this.Logger.LogInformation("Connection to {Address}:{Port} was cancelled", address, port);
            this.Manager.DisconnectAll();
            return false;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error connecting to {Address}:{Port}", address, port);
            this.Manager.DisconnectAll();
            return false;
        }
    }

    public virtual async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        => await this.ConnectAsync(this.DefaultAddress, this.DefaultPort, this.DefaultTimeout, cancellationToken);
}
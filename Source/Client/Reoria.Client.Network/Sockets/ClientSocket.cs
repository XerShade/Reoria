using LiteNetLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Network.Sockets;

namespace Reoria.Client.Network.Sockets;

/// <summary>
/// Represents a client-side network socket for connecting to remote servers.
/// </summary>
/// <remarks>
/// This class provides client networking functionality using LiteNetLib, with support for
/// configurable connection parameters, timeout handling, and comprehensive logging.
/// It inherits from the base Socket class and implements client-specific connection logic.
/// </remarks>
public class ClientSocket(ILogger<ClientSocket> logger, IConfiguration configuration) : Socket(logger, configuration)
{
    /// <summary>
    /// Gets the default server address to connect to.
    /// </summary>
    /// <remarks>
    /// Defaults to "127.0.0.1" (localhost) if not specified in configuration.
    /// This can be overridden in derived classes for different default behavior.
    /// </remarks>
    protected virtual string DefaultAddress { get; init; } = configuration["Networking:Address"] ?? "127.0.0.1";
    
    /// <summary>
    /// Gets the default port to connect to.
    /// </summary>
    /// <remarks>
    /// Defaults to port 7234 if not specified in configuration.
    /// This can be overridden in derived classes for different default ports.
    /// </remarks>
    protected virtual int DefaultPort { get; init; } = Convert.ToInt32(configuration["Networking:Port"] ?? "7234");
    
    /// <summary>
    /// Gets the default connection timeout in seconds.
    /// </summary>
    /// <remarks>
    /// Defaults to 5 seconds if not specified in configuration.
    /// This timeout is used for both synchronous and asynchronous connection attempts.
    /// </remarks>
    protected virtual int DefaultTimeout { get; init; } = Convert.ToInt32(configuration["Networking:Timeout"] ?? "5");

    /// <summary>
    /// Synchronously connects to a remote server.
    /// </summary>
    /// <param name="address">The server address to connect to.</param>
    /// <param name="port">The server port to connect to.</param>
    /// <param name="timeout">Connection timeout in seconds (default: 5).</param>
    /// <returns>True if connection was successful, false otherwise.</returns>
    /// <remarks>
    /// This is a synchronous wrapper around the asynchronous ConnectAsync method.
    /// It blocks until the connection is established, times out, or fails.
    /// </remarks>
    public override bool Connect(string address, int port, int timeout = 5)
        => this.ConnectAsync(address, port, timeout).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously connects to a remote server.
    /// </summary>
    /// <param name="address">The server address to connect to.</param>
    /// <param name="port">The server port to connect to.</param>
    /// <param name="timeout">Connection timeout in seconds (default: 5).</param>
    /// <param name="cancellationToken">Cancellation token to abort the connection attempt.</param>
    /// <returns>A task that resolves to true if connection was successful, false otherwise.</returns>
    /// <remarks>
    /// This method attempts to establish a connection to the specified server.
    /// It uses a polling mechanism to check connection status and respects cancellation tokens.
    /// The connection attempt will timeout after the specified duration.
    /// </remarks>
    public virtual async Task<bool> ConnectAsync(string address, int port, int timeout = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            // Log connection attempt with detailed parameters.
            if (this.Logger.IsEnabled(LogLevel.Information))
            {
                this.Logger.LogInformation("Attempting to connect to {Address}:{Port} with timeout {Timeout}s", address, port, timeout);
            }

            // Initiate connection using LiteNetLib manager.
            NetPeer peer = this.Manager.Connect(address, port, this.ConnectionKey);

            // Create a task to monitor connection progress.
            Task<bool> connectionTask = Task.Run(async () =>
            {
                // Poll connection status until connected, disconnected, or cancelled.
                while (peer.ConnectionState is not ConnectionState.Connected and not ConnectionState.Disconnected)
                {
                    // Check for cancellation request.
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.Logger.LogInformation("Connection attempt cancelled");
                        return false;
                    }
                    
                    // Brief delay to prevent CPU spinning.
                    await Task.Delay(100, cancellationToken);
                }
                return peer.ConnectionState == ConnectionState.Connected;
            }, cancellationToken);

            // Create timeout task to enforce connection time limit.
            Task timeoutTask = Task.Delay(timeout * 1000, cancellationToken);

            // Wait for either connection completion or timeout.
            Task completedTask = await Task.WhenAny(connectionTask, timeoutTask);
            
            // Handle timeout scenario.
            if (completedTask == timeoutTask)
            {
                this.Logger.LogWarning("Connection to {Address}:{Port} timed out after {Timeout}s", address, port, timeout);
                this.Manager.DisconnectAll();
                return false;
            }

            // Check final connection result.
            bool connected = await connectionTask;
            
            // Log successful connection.
            if (connected)
            {
                if (this.Logger.IsEnabled(LogLevel.Information))
                {
                    this.Logger.LogInformation("Successfully connected to {Address}:{Port}", address, port);
                }
            }
            else
            {
                this.Logger.LogWarning("Failed to connect to {Address}:{Port}", address, port);
            }

            return connected;
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully.
            if (this.Logger.IsEnabled(LogLevel.Information))
            {
                this.Logger.LogInformation("Connection to {Address}:{Port} was cancelled", address, port);
            }

            // Clean up connection resources.
            this.Manager.DisconnectAll();
            return false;
        }
        catch (Exception ex)
        {
            // Handle unexpected errors during connection.
            this.Logger.LogError(ex, "Error connecting to {Address}:{Port}", address, port);
            this.Manager.DisconnectAll();
            return false;
        }
    }

    /// <summary>
    /// Connects to the default server using configured parameters.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to abort the connection attempt.</param>
    /// <returns>A task that resolves to true if connection was successful, false otherwise.</returns>
    /// <remarks>
    /// This convenience method uses the default address, port, and timeout
    /// values from configuration. It's commonly used for standard client startup.
    /// </remarks>
    public virtual async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        => await this.ConnectAsync(this.DefaultAddress, this.DefaultPort, this.DefaultTimeout, cancellationToken);
}
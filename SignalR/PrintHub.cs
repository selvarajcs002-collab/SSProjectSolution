using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SSProjectSolution.SignalR
{
    [Authorize]
    public class PrintHub : Hub
    {
        private readonly ILogger<PrintHub> _logger;

        // Maps UserId to ConnectionId for routing print jobs
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();

        public PrintHub(ILogger<PrintHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                _logger.LogInformation("Print Agent connected: UserId {UserId}, ConnectionId {ConnectionId}", userId, Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning("Print Agent connected but UserId is missing from token. ConnectionId {ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.TryRemove(userId, out _);
                _logger.LogInformation("Print Agent disconnected: UserId {UserId}", userId);
            }

            if (exception != null)
            {
                _logger.LogError(exception, "Print Agent connection dropped due to an error.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Heartbeat(string version, string printerStatus)
        {
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;
            _logger.LogInformation("Heartbeat from Print Agent (UserId: {UserId}): Version {Version}, Printer Status: {PrinterStatus}", userId, version, printerStatus);
            // Can update a health check table here if desired
        }

        // Returns the active connection ID for a given user, if any
        public static string? GetConnectionIdForUser(string userId)
        {
            _userConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }
    }
}

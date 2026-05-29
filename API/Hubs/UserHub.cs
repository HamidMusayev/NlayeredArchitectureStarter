using CORE.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

/// <summary>
///     SignalR hub stub for real-time user notifications. Requires JWT bearer authentication
///     and is exposed with the permissive CORS policy so browser clients can connect.
///     Extend with typed hub methods (<c>JoinGroup</c>, <c>SendAsync</c>, etc.) as needed.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[EnableCors(Constants.EnableAllCorsName)]
public class UserHub : Hub
{
    // Stub hub kept as a starter example.
    // Extend with JoinGroup / SendAsync etc. as needed.

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}
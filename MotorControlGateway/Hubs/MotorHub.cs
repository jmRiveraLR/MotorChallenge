
using Microsoft.AspNetCore.SignalR;
using MotorControlGateway.Services;

namespace MotorControlGateway.Hubs
{
	public class MotorHub:Hub
	{
		private readonly ILogger<MotorHub> logger;

		public MotorHub( ILogger<MotorHub> log)
		{
			logger = log;

		}

		public override  Task OnConnectedAsync() {

			logger.LogInformation("successfull connection to MotorHub:{ConnectionId}",Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            logger.LogInformation("successfull disconnetion to MotorHub: {ConnectionId}", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}

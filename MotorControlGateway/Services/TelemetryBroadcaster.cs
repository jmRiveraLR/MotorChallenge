using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using MotorControlGateway.Hubs;
using MotorControlGateway.Services;

namespace MotorControlGateway.Services
{
    public class TelemetryBroadcaster : BackgroundService
    {
        private readonly IHubContext<MotorHub> contexthub;
        private readonly MotorSimulator motor;
        private readonly ILogger<TelemetryBroadcaster> logger;
        private bool overheatNotified = false; // ⬅️ para evitar spam

        public TelemetryBroadcaster(
            IHubContext<MotorHub> hubContext,
            MotorSimulator nmotor,
            ILogger<TelemetryBroadcaster> log)
        {
            contexthub = hubContext;
            motor = nmotor;
            logger = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("TelemetryBroadcaster iniciado.");
            while (!stoppingToken.IsCancellationRequested)
            {
                // Avanza la simulación 
                var status = motor.Update();  

                await contexthub.Clients.All.SendAsync("ReceiveTelemetry", status, stoppingToken);

                if (status.Overheated && !overheatNotified)
                {
                    overheatNotified= true;

                    await contexthub.Clients.All.SendAsync("Overheating", new
                    {
                        temperature = status.Temperature,
                        message = "Overheat detected. Motor stopped."
                    },stoppingToken);
                }
                else if (!status.Overheated && overheatNotified)
                {
                    overheatNotified = false;
                }

                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}

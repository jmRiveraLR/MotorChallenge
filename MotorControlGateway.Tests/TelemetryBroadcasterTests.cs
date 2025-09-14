using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using MotorControlGateway.Hubs;
using MotorControlGateway.Services;
using Xunit;

namespace MotorControlGateway.Tests
{
    public class TelemetryBroadcasterTests
    {
        [Fact]
        public async Task Broadcasts_Telemetry_And_Overheat_Warning()
        {
            // Arrange
            var motor = new MotorSimulator();
            motor.SetTempLimit(25); // force overheating faster
            motor.SetSpeed(100);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

            var mockHubContext = new Mock<IHubContext<MotorHub>>();
            mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

            var mockLogger = new Mock<ILogger<TelemetryBroadcaster>>();

            var broadcaster = new TelemetryBroadcaster(
                mockHubContext.Object,
                motor,
                mockLogger.Object
            );

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(500); // stop quickly after a few ticks

            // Act
            await broadcaster.StartAsync(cts.Token);

            // Assert
            mockClientProxy.Verify(
                c => c.SendCoreAsync("ReceiveTelemetry",
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            mockClientProxy.Verify(
                c => c.SendCoreAsync("Overheating",
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
    }
}

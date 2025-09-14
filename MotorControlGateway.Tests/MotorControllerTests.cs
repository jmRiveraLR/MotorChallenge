using Microsoft.AspNetCore.Mvc;
using MotorControlGateway.Controllers;
using MotorControlGateway.Models;
using MotorControlGateway.Services;
using Xunit;

namespace MotorControlGateway.Tests
{
    public class MotorControllerTests
    {
        private readonly MotorController testController;
        private readonly MotorSimulator testMotor;

        public MotorControllerTests()
        {
            testMotor = new MotorSimulator();
            testController = new MotorController(testMotor);
        }

        // tests to see is i get expected responses
        [Fact]
        public void GetStatus_Returns_Status()
        {
            var result = testController.GetStatus();
            Assert.NotNull(result.Value);
            Assert.IsType<MotorStatus>(result.Value);
        }

        [Fact]
        public void SetSpeed_Updates_TargetSpeed()
        {
            var result = testController.SetSpeed(80) as OkObjectResult;
            var status = result?.Value as MotorStatus;

            Assert.NotNull(status);
            Assert.Equal(80, status.TargetSpeed);
        }

        [Fact]
        public void SetMode_Changes_Mode()
        {
            var result = testController.SetMode("sport") as OkObjectResult;
            var status = result?.Value as MotorStatus;

            Assert.NotNull(status);
            Assert.Equal("sport", status.Mode);
        }

        [Fact]
        public void Stop_Sets_Stopped_Flag()
        {
            var result = testController.Stop() as OkObjectResult;
            var status = result?.Value as MotorStatus;

            Assert.NotNull(status);
            Assert.True(status.Stopped);
        }

        [Fact]
        public void SetTempLimit_Updates_Limit()
        {
            var result = testController.SetTempLimit(40) as OkObjectResult;
            var status = result?.Value as MotorStatus;

            Assert.NotNull(status);
            Assert.Equal(40, status.TemperatureLimit);
        }
    }
}

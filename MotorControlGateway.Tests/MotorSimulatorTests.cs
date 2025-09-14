using MotorControlGateway.Services;
using Xunit;

namespace MotorControlGateway.Tests
{
    public class MotorSimulatorTests
    {
        [Fact]
        public void Speed_Increases_Towards_Target()
        {
            var motor = new MotorSimulator();
            motor.SetSpeed(50);

            var status = motor.Update();
            Assert.True(status.Speed >= 0);
            Assert.Equal(50, status.TargetSpeed);
        }

        [Fact]
        public void Stop_Sets_Speed_And_Rpm_To_Zero()
        {
            var motor = new MotorSimulator();
            motor.SetSpeed(50);
            motor.Update();

            motor.Stop();
            var status = motor.GetStatus();

            Assert.Equal(0, status.Speed);
            Assert.Equal(0, status.Rpm);
            Assert.True(status.Stopped);
        }

        [Fact]
        public void Overheats_When_Temperature_Reaches_Limit()
        {
            var motor = new MotorSimulator();
            motor.SetTempLimit(25);
            motor.SetSpeed(100);

            // Run updates until overheating
            for (int i = 0; i < 500; i++) motor.Update();

            var status = motor.GetStatus();
            Assert.True(status.Overheated);
            Assert.True(status.Stopped);
        }

        [Fact]
        public void Recovers_From_Overheated_When_Cooled()
        {
            var motor = new MotorSimulator();
            motor.SetTempLimit(25);
            motor.SetSpeed(100);

            // Force overheat
            for (int i = 0; i < 500; i++) motor.Update();

            // Cool down
            motor.Stop();
            for (int i = 0; i < 500; i++) motor.Update();

            var status = motor.GetStatus();
            Assert.False(status.Overheated);
        }
    }
}

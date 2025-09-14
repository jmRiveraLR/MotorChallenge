using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MotorControlGateway.Models;
using MotorControlGateway.Services;

namespace MotorControlGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotorController : ControllerBase
    {
        private readonly MotorSimulator motor;
        public MotorController(MotorSimulator simulador)
        {
            motor = simulador;
        }

        //requests

        //GET api/motor/status
        [HttpGet("status")]
        public ActionResult<MotorStatus> GetStatus() => motor.GetStatus();

        // POST /api/motor/speed
        [HttpPost("speed")]
        public IActionResult SetSpeed([FromBody] int speed)
        {
            motor.SetSpeed(speed);
            return Ok(motor.GetStatus());
        }

        // POST /api/motor/mode
        [HttpPost("mode")]
        public IActionResult SetMode([FromBody] string mode)
        {
            motor.SetMode(mode);
            return Ok(motor.GetStatus());
        }

        // POST /api/motor/stop
        [HttpPost("stop")]
        public IActionResult Stop()
        {
            motor.Stop();
            return Ok(motor.GetStatus());
        }
        // POST /api/motor/stop
        [HttpPost("templimit")]
        public IActionResult SetTempLimit([FromBody] double limit)
        {   
            motor.SetTempLimit(limit);
            return Ok(motor.GetStatus());
        }
    }
}

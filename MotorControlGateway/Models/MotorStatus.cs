namespace MotorControlGateway.Models
{
    public class MotorStatus
    {

        public int Speed { get; set; } // current speed of the car , i assume KM/H

        public int TargetSpeed { get; set; } // speed i want it to go 

        public int Rpm { get; set; } // revolutions per minute

        public double Temperature { get; set; } // Temperature of the motor , in C

        public double Output { get; set; } // Current output of the electric motor  i assume volts

        public String Mode { get; set; } = "eco"; // current mode of the motor , there are 3 eco , normal and sport

        public bool Stopped { get; set; }

        public bool Overheated { get; set; } // says if the motor if overheated

        public double TemperatureLimit { get; set; } // temperature limit for the motor, if reached it must stop
    }
}

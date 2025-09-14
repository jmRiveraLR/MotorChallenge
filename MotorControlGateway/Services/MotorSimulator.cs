using System.Diagnostics.Eventing.Reader;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using MotorControlGateway.Models;

namespace MotorControlGateway.Services
{
	public class MotorSimulator
	{
        private readonly object motorLock = new object();
       
        private readonly MotorStatus status = new();


        //sim base parameters , change to taste
        private const double TempUpPerTick = 0.4;    // °C por tick si hay movimiento
        private const double TempDownPerTick = 0.3;  // °C por tick si está parado
        private const double TempMin = 20.0; // ambient temp
        private double TempLimit = 50; // default temperature limit
        private  double TempSafe = 35;   // temperature to exit overheated state

        public MotorSimulator()
        {
            status.Speed = 0;
            status.TargetSpeed = 0;
            status.Rpm = 0;
            status.Temperature = TempMin;
            status.Mode = "normal";
            status.Stopped = false;
            status.TemperatureLimit = TempLimit;
            status.Output = 0.0;
        }
        
        public MotorStatus GetStatus()
        {
            lock (motorLock)
            {
                return new MotorStatus
                {
                    Speed = status.Speed,
                    TargetSpeed = status.TargetSpeed,
                    Rpm = status.Rpm,
                    Temperature = status.Temperature,
                    Mode = status.Mode,
                    Stopped = status.Stopped,
                    TemperatureLimit= status.TemperatureLimit,
                    Output = status.Output,
                    Overheated = status.Overheated
                };
            }
        }

        public void SetTempLimit(double limit)
        {
            if (limit < TempMin) limit = TempMin;          // seguridad
            if (limit > 70) limit = 70;
            lock (motorLock)
            {
                TempLimit = limit;
                status.TemperatureLimit = limit;
                TempSafe = Math.Round(limit*0.6);
            }
        }

        public void SetMode(string mode)
        {
           
            lock (motorLock)
            {
                status.Mode = mode.ToLower();
            }
        }

        public void SetSpeed(int speed)
        {
            if(speed < 0) speed = 0; // in case someone tries to use method with a neg speed

            lock (motorLock)
            {
                status.TargetSpeed = speed; // incase someone tries to enter a negative number
                status.Stopped = false;
                                          
            }



        }

        public void Stop()
        {

            lock (motorLock)
            {
                status.Stopped = true;
                status.TargetSpeed = 0;
                status.Mode = "normal"; // i assume that eco is the lowest setting
                status.Rpm = 0;
                status.Speed = 0;
                status.Output= 0.0;   
            }
        }

        public MotorStatus Update()
        {
            lock (motorLock)
            {
                if (status.Stopped)
                {
                    status.Speed = 0;
                    status.Rpm = 0;
                }
                else
                {
                    //  acceleration factor by mode
                    double modeFactor = status.Mode switch
                    {
                        "eco" => 0.6,
                        "sport" => 1.4,
                        "normal" => 1.0,
                        _ => 1.0
                    };

                    int step = Math.Max(1, (int)Math.Round(4 * modeFactor));
                    int diff = status.TargetSpeed - status.Speed;

                    if (diff != 0)
                    {
                        int delta = Math.Sign(diff) * Math.Min(Math.Abs(diff), step);
                        status.Speed += delta;
                        if (status.Speed < 0) status.Speed = 0;
                    }

                    status.Rpm = Math.Max(0, status.Speed * 100);

                    // Output: proporcional a speed con factor por modo
                    // (ajústalo a tu gusto)
                    double baseOutput = status.Speed * 1.1; // base
                    double outputFactor = status.Mode switch
                    {
                        "eco" => 0.7,     // menos potencia
                        "normal" => 1.3,   // estandar
                        "sport" => 1.3,   // más potencia
                        _ => 1.0
                    };
                    status.Output = Math.Round(baseOutput * outputFactor, 2);
                }


                // Temperature logic
                if (status.Speed > 0)
                {
                    status.Temperature += TempUpPerTick;
                }
                else
                {
                    status.Temperature -= TempDownPerTick;
                }

                if (status.Temperature < TempMin)
                    status.Temperature = TempMin;

                //  overheated checks
                if (status.Temperature >= TempLimit && !status.Overheated)
                {
                    Stop();
                    status.Overheated = true;
                }
                else if (status.Overheated && status.Temperature <= TempSafe)
                {
                    status.Overheated = false;
                }

                return GetStatus();
            }
        }

        }
    }

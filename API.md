## Base URL

https://localhost:7192/api/motor


# Endpoints

### GET /status

Returns current motor status.

expected response example:
{
  "speed": 30,
  "targetSpeed": 65,
  "rpm": 3000,
  "temperature": 45.3,
  "output": 120.5,
  "mode": "sport",
  "stopped": false,
  "overheated": false,
  "temperatureLimit": 70
}

### POST /speed

Sets motor speed.

Requests an int for speed

Responds with Motor Status

### POST /mode

Sets motor mode.

Requests String for mode

Responds with Motor Status

### POST /stop

Immediately stops the motor

responds with motor status

### POST /templimit

Set maximum allowed temperature.

request an int for setting a new limit

reponds with motor status

## SignalR Hub

### /motorHub

Events in the motor hub:

- ReceiveTelemetry → sends live MotorStatus object every 2 seconds.

- Overheating → fired once when motor overheats, with payload:
    {
    "temperature": 72.1,
    "message": "Overheat detected. Motor stopped."
    }
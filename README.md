# Motor Control Gateway

## Overview
This project simulates a Motor Control Gateway using ASP.NET Core + SignalR as the backend and a React + Vite dashboard as the frontend.
Its a motor simulation with telemetry broadcasting for real-time visualization.

## Features
- Motor simulator with speed, RPM, temperature, mode, and output.

- Temperature safety logic with overheating detection and cool-down recovery.

- REST API for motor control (speed, mode, stop, templimit).

- SignalR hub broadcasting real-time telemetry to all clients.

- React dashboard:
            Slider to set speed.
            Dropdown to set mode (Eco / Normal / Sport).
            Stop button for emergency shutdown.
            Textbox to set temperature limit.
            Line chart displaying speed, temperature, and output.
            Popup warning when overheated.

## Prerequisites
- .NET 8 SDK
- Nodes.js 18+
- npm or yarn
- Postman (Optional)

## Installation and set up

1. Clone the repository:
    ```bash
    git clone https://github.com/your-username/MotorControlGateway.git
    ```
1.1  Backend: MotorControlGateway

    cd MotorControlGateway
    dotnet restore
    dotnet run

API will start at https://localhost:7192/swagger

1.2. Frontend: MotorDashboard

    cd MotorDashboard
    npm install
    npm run dev

API will start at https://localhost:7192.

You can ajust this.

1.3 Runing Tests

The unit test suite includes tests for the motor simulator, controller endpoints, and the telemetry broadcasting service using a mocked SignalR hub.

    cd MotorControlGateway.Tests
    dotnet test





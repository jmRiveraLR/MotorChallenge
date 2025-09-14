
# Design Decisions
ASP.NET Core + SignalR chosen for real-time telemetry instead of polling, reducing latency and overhead.

BackgroundService (TelemetryBroadcaster) cleanly separates simulation loop from API controllers, following ASP.NET hosting patterns.

Singleton MotorSimulator ensures a single shared motor state across controllers and hub.

React + Recharts chosen for an interactive, simple dashboard with real-time charting.

Simulation model simplified: RPM directly proportional to speed, temperature increase linear. for more advance simulations it would be needed to create more changing variable like friction, air resistance , temperature coefficient, material classification and heat disipation of the motor among other thing

In real-world scenarios, physics would be more complex to measure and require specialized controllers.

States like overheating  and stopped is in-memory only; persistence is not implemented cause the scope is limited.


# Architecture

MotorSimulator encapsulates all motor logic (speed ramping, mode factors, temperature control, overheating).

MotorController exposes REST endpoints for external commands.

MotorHub and TelemetryBroadcaster handle real-time data distribution.

React Dashboard represents the “vehicle computer” consuming telemetry.


# Patterns & Practices

Dependency Injection for services.

DTO / POCO model (MotorStatus) separating state from logic.

Observer Pattern via SignalR (clients subscribe to telemetry).

Unit of Work simulated through singleton + background task.

#  Trade offs
1. the use  of a singleton  means that theres only a single instance , that means no multi-user simulation support. If multiple clients connect, they all see the same state (not isolated). This was acceptable since the challenge focuses on a single simulated motor, not multi-client case was proposed.

2. the telemetry broadcasting with signalR provides a simple and consistent way to get real time updates to the client, but the fixed tick interval maybe inefficient if no one is connected.

3. the way the overheating logic is implemented with templimit and tempSafe allows  a simplified implementation of the logic and ties the safety rules to the simulation , but a more complex desing or system could separe the handling of the physic of the simulation and safety regulations of the model.

4. the use of debouncing in the useEffect hooks and the delay of 300ms in the front end  to avoid constant API call and a smother user experience but it reduces the responsiveness

5. the simplified and minimalistic dashboard was implemented that way cause of the simple simple simulation logic and the calarity for the user,also is easy to mantain , edit and expand in case of changes to the logic or introduction of new variables.however it has no advanced animations , its not ajusted for mobile use , and it doesnt feel sooth enough for production standar
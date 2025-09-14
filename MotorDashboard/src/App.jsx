import { useEffect, useRef, useState } from "react";
import "./App.css";
import { connection } from "./services/signalr";
import {
  getStatus,
  setSpeed,
  setMode,
  stopMotor,
  setTempLimit as setTempLimitApi
} from "./services/api";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from "recharts";

export default function App() {
  const [status, setStatus] = useState(null);
  const [desiredSpeed, setDesiredSpeed] = useState(0);
  const [desiredMode, setDesiredMode] = useState("normal");
  const [history, setHistory] = useState([]);
  const [showOverheat, setShowOverheat] = useState(false);
  const [tempLimit, setTempLimitState] = useState(70);

  // refs
  const adjustingSpeedRef = useRef(false);
  const initializedDesiredRef = useRef(false);
  const debounceTimerRef = useRef(null);
  const tempDebounceRef = useRef(null);

  useEffect(() => {
    let isMounted = true;

    const telemetryHandler = (data) => {
      if (!isMounted) return;
      setStatus(data);

      // sincroniza controles solo la primera vez
      if (!initializedDesiredRef.current && data) {
        setDesiredSpeed(Number(data.targetSpeed ?? 0));
        setDesiredMode(data.mode ?? "normal");
        setTempLimitState(Number(data.tempLimit ?? 20));
        initializedDesiredRef.current = true;
      }

      // histórico para la gráfica
      setHistory((prev) => [
        ...prev.slice(-49),
        {
          t: new Date().toLocaleTimeString(),
          speed: Number(data?.speed ?? 0),
          temperature: Number(data?.temperature ?? 0),
          output: Number(data?.output ?? 0),
        },
      ]);
    };

    const overheatHandler = () => {
      setShowOverheat(true);
    };

    (async () => {
      try {
        if (connection.state === "Disconnected") {
          await connection.start();
          console.log("✅ Connected to SignalR");
        }

        connection.off("ReceiveTelemetry", telemetryHandler);
        connection.on("ReceiveTelemetry", telemetryHandler);

        connection.off("Overheating", overheatHandler);
        connection.on("Overheating", (payload) => {
          console.warn("🔥 Overheating event received:", payload);
          setShowOverheat(true);
        });

        const res = await getStatus();
        if (!isMounted) return;
        setStatus(res.data);
        setDesiredSpeed(Number(res.data?.targetSpeed ?? 0));
        setDesiredMode(res.data?.mode ?? "normal");
        setTempLimitState(Number(res.data?.tempLimit ?? 20));
      } catch (e) {
        console.error("❌ Connection Error:", e);
      }
    })();

    return () => {
      isMounted = false;
      connection.off("ReceiveTelemetry", telemetryHandler);
      connection.off("Overheating", overheatHandler);

      if (debounceTimerRef.current) clearTimeout(debounceTimerRef.current);
      if (tempDebounceRef.current) clearTimeout(tempDebounceRef.current);
    };
  }, []);

  useEffect(() => {
  if (status?.overheated) {
    setDesiredSpeed(0);   // reset local slider state
  }
}, [status?.overheated]);

  // speed handler
  const applySpeed = async (value) => {
    await setSpeed(value);
  };

  // mode handler
  const applyMode = async () => {
    await setMode(desiredMode);
  };

  // stop handler
  const handleStop = async () => {
    await stopMotor();
  };

  // temp limit handler with debounce
  const onTempLimitChange = (value) => {
    const v = Math.max(20, Math.min(70, Number(value) || 20));
    setTempLimitState(v);

    if (tempDebounceRef.current) clearTimeout(tempDebounceRef.current);
    tempDebounceRef.current = setTimeout(async () => {
      try {
        await setTempLimitApi(v);         // update backend
        const res = await getStatus();    // refresh
        setStatus(res.data);
      } catch (err) {
        console.error("Error updating temp limit:", err);
      }
    }, 300);
  };

  // Slider with debounce
  const onSpeedChange = (val) => {
    setDesiredSpeed(val);
    if (debounceTimerRef.current) clearTimeout(debounceTimerRef.current);
    debounceTimerRef.current = setTimeout(() => {
      if (!adjustingSpeedRef.current) applySpeed(val);
    }, 300);
  };

  const onSpeedMouseDown = () => {
    adjustingSpeedRef.current = true;
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
      debounceTimerRef.current = null;
    }
  };

  const onSpeedMouseUp = () => {
    adjustingSpeedRef.current = false;
    applySpeed(desiredSpeed);
  };

  return (
    <div className="app">
      <h1 className="title">🚗 Motor Dashboard</h1>

      {!status ? (
        <p className="loading">Loading State...</p>
      ) : (
        <>


          {/* Status Grid */}
          <div className="status-grid">
            <div><b>Actual Speed</b><div>{Number(status.speed ?? 0)} km/h</div></div>
            <div><b>RPM</b><div>{Number(status.rpm ?? 0)}</div></div>
            <div><b>Temperature</b><div>{Number(status.temperature ?? 0).toFixed(1)} °C</div></div>
            <div><b>Mode</b><div>{status.mode ?? "normal"}</div></div>
            <div><b>Output</b><div>{Number(status.output ?? 0).toFixed(2)} V </div></div>
            <div><b>Temp Limit</b><div>{Number(status.temperatureLimit ?? 0)} °C</div></div>

          </div>

          {/* Controls */}
          <div className="controls">


            {/* Speed Slider */}
            <div className="speed-control">
              <label><b>Speed: {desiredSpeed} km/h</b></label>
              <input
                type="range"
                min={0}
                max={100}
                step={5}
                value={status?.overheated ? 0 : desiredSpeed}    // resetea a 0 si sobrecalienta
                onChange={(e) => onSpeedChange(parseInt(e.target.value))}
                onMouseDown={onSpeedMouseDown}
                onMouseUp={onSpeedMouseUp}
                onTouchStart={onSpeedMouseDown}
                onTouchEnd={onSpeedMouseUp}
                disabled={status?.overheated}   // disable si sobrecalienta
              />
            </div>

            {/* Temp Limit Textbox */}
            <div className="limits-row">
              <div className="limit-item">
                <label><b>Temp Limit (°C)</b></label>
                <input
                  type="text"
                  value={tempLimit}
                  onChange={(e) => {
                    // allow user to type anything numeric
                    const onlyNums = e.target.value.replace(/\D/g, "");
                    setTempLimitState(onlyNums);
                  }}
                  onBlur={() => {
                    // valida valor y ajusta a rango limite 20-70 si necesario
                    let num = Number(tempLimit);
                    if (isNaN(num)) num = 20;
                    if (num < 20) num = 20;
                    if (num > 70) num = 70;
                    setTempLimitState(num);

                    // send to backend with debounce
                    if (tempDebounceRef.current) clearTimeout(tempDebounceRef.current);
                    tempDebounceRef.current = setTimeout(async () => {
                      try {
                        await setTempLimitApi(num);
                        const res = await getStatus();
                        setStatus(res.data);
                      } catch (err) {
                        console.error("Error updating temp limit:", err);
                      }
                    }, 300);
                  }}
                  placeholder="Enter °C"
                />
              </div>
            </div>
            <select value={desiredMode} onChange={(e) => setDesiredMode(e.target.value)}>
              <option value="eco">Eco</option>
              <option value="normal">Normal</option>
              <option value="sport">Sport</option>
            </select>
            <button onClick={applyMode}>Change Mode</button>

            <button className="stop-btn" onClick={handleStop}>🛑 Stop Car</button>
          </div>

          {/* Chart */}
          <div className="chart-container">
            <LineChart width={880} height={320} data={history}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="t" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="speed" name="Speed Km/H" stroke="#e53935" dot={false} isAnimationActive={false} />
              <Line type="monotone" dataKey="temperature" name="Temperature °C" stroke="#1e88e5" dot={false} isAnimationActive={false} />
              <Line type="monotone" dataKey="output" name="Output V" stroke="#25e51e" dot={false} isAnimationActive={false} />
            </LineChart>
          </div>
          {/* Overheat Popup */}
          {showOverheat && (
            <div className="popup-overlay">
              <div className="popup">
                <div className="popup-title">🔥🔥🔥 Overheated 🔥🔥🔥</div>
                <p>The Motor overheated and such the car has been stopped, Please wait until temperature reaches safe metric to start motor again.</p>
                <button onClick={() => setShowOverheat(false)}>Understood</button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}

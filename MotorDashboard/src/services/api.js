import axios from "axios";

const api = axios.create({
  baseURL: "https://localhost:7192/api/motor",
});

export const getStatus = () => api.get("/status");
export const setSpeed = (speed) => api.post("/speed", speed, {
  headers: { "Content-Type": "application/json" }
});
export const setMode = (mode) => api.post("/mode", JSON.stringify(mode), {
  headers: { "Content-Type": "application/json" }
});
export const stopMotor = () => api.post("/stop");

export const setTempLimit = (limit) => api.post("/templimit", limit, {
  headers: { "Content-Type": "application/json" }
});
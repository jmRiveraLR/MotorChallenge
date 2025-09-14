import * as signalR from "@microsoft/signalr";

 export const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7192/motorHub") // remember to change if back endpoint changes
    .withAutomaticReconnect()
    .build();
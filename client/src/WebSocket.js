import * as signalR from "@microsoft/signalr";

const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5294/board")
    .build();

export default hubConnection;
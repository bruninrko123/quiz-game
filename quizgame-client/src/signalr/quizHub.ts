import * as signalR from "@microsoft/signalr";

const HUB_URL = "http://localhost:5270/quizhub";

export const quizHubConnection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build();

    


import * as signalR from "@microsoft/signalr";

const HUB_URL = `${import.meta.env.VITE_API_URL}/quizhub`;

export const quizHubConnection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build();

    


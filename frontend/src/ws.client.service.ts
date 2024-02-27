import {EventEmitter, Injectable} from "@angular/core";
import {WebsocketSuperclass} from "./models/WebsocketSuperclass";
import {Router} from "@angular/router";
import {BaseDto} from "./models/baseDto";
import {MessageService} from "primeng/api";
import {ServerAuthenticatesUser} from "./models/serverAuthenticatesUser";
import {ServerAddsClientToRoom} from "./models/serverAddsClientToRoom";
import {ServerBroadcastsMessageToClientsInRoom} from "./models/serverBroadcastsMessageToClientsInRoom";
import {ServerSendsErrorMessageToClient} from "./models/serverSendsErrorMessageToClient";
import {ServerNotifiesClientsInRoomSomeoneHasJoinedRoom} from "./models/serverNotifiesClientsInRoomSomeoneHasJoinedRoom";
import {Message, Room} from "./models/entities";
import {ServerSendsImageAnalysisToClient} from "./models/imageDetectionModels";
import {environment} from "./environments/environment";
import {AnalysisResult} from "./models/imageDetectionModels";

@Injectable({providedIn: 'root'})
export class WebSocketClientService {


  public roomsWithMessages: Map<number, Message[]> = new Map<number, Message[]>();
  public roomsWithNumberOfConnections: Map<number, number> = new Map<number, number>();


  public socketConnection: WebsocketSuperclass;

  constructor(public messageService: MessageService) {
    this.socketConnection = new WebsocketSuperclass(environment.websocketBaseUrl);
    this.handleEventsEmittedByTheServer()
  }

  handleEventsEmittedByTheServer() {
    this.socketConnection.onmessage = (event) => {
      const data = JSON.parse(event.data) as BaseDto<any>;
      console.log("Received: " + JSON.stringify(data));
      //@ts-ignore
      this[data.eventType].call(this, data);
    }
  }


  ServerAddsClientToRoom(dto: ServerAddsClientToRoom) {
    this.roomsWithMessages.set(dto.roomId!, dto.messages!.reverse());
    this.roomsWithNumberOfConnections.set(dto.roomId!, dto.liveConnections!);
  }

  ServerAuthenticatesUser(dto: ServerAuthenticatesUser) {
    this.messageService.add({life: 2000, detail: 'Authentication successful!'});
    localStorage.setItem("jwt", dto.jwt!);
  }

  ServerAuthenticatesUserFromJwt(dto: ServerAuthenticatesUser) {
    this.messageService.add({life: 2000, summary: 'success', detail: 'Authentication successful!'});
  }

  ServerBroadcastsMessageToClientsInRoom(dto: ServerBroadcastsMessageToClientsInRoom) {
    this.roomsWithMessages.get(dto.roomId!)!.push(dto.message!);
    this.messageService.add({life: 2000, summary: '📬', detail: 'New message!'});
  }

  ServerNotifiesClientsInRoomSomeoneHasJoinedRoom(dto: ServerNotifiesClientsInRoomSomeoneHasJoinedRoom) {
    this.messageService.add({
      life: 2000,
      summary: '🧨',
      detail: "New user joined: " + dto.userEmail
    });
    this.roomsWithNumberOfConnections.set(dto.roomId!, this.roomsWithNumberOfConnections.get(dto.roomId!)! + 1);
  }


  ServerSendsErrorMessageToClient(dto: ServerSendsErrorMessageToClient) {
    this.messageService.add({life: 2000, severity: 'error', summary: '⚠️', detail: dto.errorMessage}); //todo implement with err handler
  }

  public onImageAnalysisReceived: EventEmitter<AnalysisResult> = new EventEmitter();
  ServerSendsImageAnalysisToClient(dto: ServerSendsImageAnalysisToClient) {
    this.onImageAnalysisReceived.emit(dto.result!);
  }

}

import {Injectable} from "@angular/core";
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

@Injectable({providedIn: 'root'})
export class WebSocketClientService {


  public roomsWithMessages: Map<number, Message[]> = new Map<number, Message[]>();
  public roomsWithConnections: Map<number, number> = new Map<number, number>();




  public socketConnection: WebsocketSuperclass;

  constructor(public messageService: MessageService) {
    this.socketConnection = new WebsocketSuperclass("ws://localhost:8181");
    this.handleEvent()
  }

  handleEvent() {
    this.socketConnection.onmessage = (event) => {
      const data = JSON.parse(event.data) as BaseDto<any>;
      console.log("Received: " + JSON.stringify(data));
      //@ts-ignore
      this[data.eventType].call(this, data);
    }
  }


  ServerAddsClientToRoom(dto: ServerAddsClientToRoom) {
    this.roomsWithMessages.set(dto.roomId!, dto.messages!.reverse());
    this.roomsWithConnections.set(dto.roomId!, dto.liveConnections!);
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
      detail: "New user joined: " + dto.user?.email
    });
    this.roomsWithConnections.set(dto.roomId!, this.roomsWithConnections.get(dto.roomId!)! + 1);
  }


  ServerSendsErrorMessageToClient(dto: ServerSendsErrorMessageToClient) {
    this.messageService.add({life: 2000, severity: 'error', summary: '⚠️', detail: dto.errorMessage}); //todo implement with err handler
  }


}

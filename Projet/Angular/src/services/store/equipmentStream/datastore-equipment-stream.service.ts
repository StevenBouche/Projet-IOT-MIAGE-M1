import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import MotorRobot from 'src/models/MotorRobot';

@Injectable({
  providedIn: 'root'
})
export class DatastoreEquipmentStreamService {

  //Store
  private store: {
    idEquipment: string;
  } = {
    idEquipment: undefined,
  }

  private _dataVideo = new BehaviorSubject<string>(undefined);
  private _dataMotor = new BehaviorSubject<MotorRobot>(undefined);
  
  readonly dataVideo = this._dataVideo.asObservable();
  readonly dataMotor = this._dataMotor.asObservable();

  private connection: signalR.HubConnection = undefined;

  constructor() { }

  public async setEquipmentIdSelected(id: string){

    //Set current idEquipment
    this.store.idEquipment = id;

    //Build signalR connection and try to connect
    await this.buildConnection();
  }

  private async buildConnection(){

    //if we are already connected stop the latest connection
    if(this.isConnected()){
      await this.connection.stop();
      this.connection = undefined;
    }

    //dont connect if id is undefined
    if(this.store.idEquipment === undefined)
      return;

    //build new hub with idEquipment
    this.connection = new signalR.HubConnectionBuilder()
      .configureLogging(signalR.LogLevel.Information)
      .withUrl(`${environment.wsBaseUrl}/equipmentStream`)
      .withHubProtocol(new MessagePackHubProtocol())
      .build();

    this.connection.onclose((error) => {
      console.log(error)
      setTimeout(this.connectSignalR, 5000);
    });

    //try to connect
    await this.connectSignalR();
  }

  private async connectSignalR(): Promise<void> {

    if(this.isConnected())
      return;

    try {
        await this.connection.start();
        this.subscribeSignalR();
    } catch (err) {
        console.log(err);
        console.log("Trying to reconnect websocket !");
        setTimeout(this.connectSignalR, 5000);
    }
  }

  private subscribeSignalR() {

    if(!this.isConnected())
      return;

    this.subscribeStreamVideo();
    this.subscribeStreamMotor();

  }

  private subscribeStreamVideo(){

    if(!this.isConnected() && this.store.idEquipment != undefined)
      return;

    var callbackStreamError = () => this.subscribeStreamVideo();

    this.connection.stream("EquipmentStream", this.store.idEquipment).subscribe({
      next: (item) => {
          this._dataVideo.next(item);
      },
      complete: () => {
          this._dataVideo.next(undefined);
          setTimeout(callbackStreamError, 5000);
      },
      error: (err) => {
          this._dataVideo.next(undefined);
          setTimeout(callbackStreamError, 5000);
      }
    });
  }

  private subscribeStreamMotor(){

    if(!this.isConnected() && this.store.idEquipment != undefined)
      return;
      
    var callbackStreamError = () => this.subscribeStreamMotor();

    this.connection.stream("EquipmentStreamMotor", this.store.idEquipment).subscribe({
      next: (item) => {
          this._dataMotor.next(item);
      },
      complete: () => {
          this._dataMotor.next(undefined);
          setTimeout(callbackStreamError, 5000);
      },
      error: (err) => {
          this._dataMotor.next(undefined);
          setTimeout(callbackStreamError, 5000);
      }
    });
  }

  private isConnected(){
    return this.connection != undefined && this.connection.state === signalR.HubConnectionState.Connected;
  }


}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import EquipmentState from 'src/models/EquipmentState';

@Injectable({
  providedIn: 'root'
})
export class DatastoreDashboardService {

  //Store
  private store: {
    equipments: Array<EquipmentState>;
  } = {
    equipments: new Array<EquipmentState>(),
  }

  //Behavior
  private _equipments = new BehaviorSubject<Array<EquipmentState>>(this.store.equipments);

  //Observable
  readonly equipments = this._equipments.asObservable();

  //Other
  private connection: signalR.HubConnection;

  constructor(private http: HttpClient) { 
    //Build a new HubConnection signalR
    this.connection = new signalR.HubConnectionBuilder()  
      .configureLogging(signalR.LogLevel.Information)  
      .withUrl(`${environment.wsBaseUrl}/equipments`)  
      .build(); 
    //Setup callback when clodes and start connection
    this.connection.onclose(() => this.connectSignalR());
    this.connectSignalR();
  }

  private async connectSignalR(): Promise<void> {

    if(this.isConnected())
      return;

    try {
        //Subscribe events hub
        this.subscribeSignalR();
        await this.connection.start(); 
    } catch (err) {
        console.log(err);
        console.log("Trying to reconnect websocket !");
        setTimeout(() => this.connectSignalR(), 5000);
        return;
    }

    //When we are connected get datas equipments and set
    this.setEquipments(await this.getEquipmentsSignalR());
  }

  private subscribeSignalR() {
    this.connection.on("onChangeEquipments", (data) => this.setEquipments(data));
  }

  private setEquipments(equipments: Array<EquipmentState>){
    this.store.equipments = equipments;
    this._equipments.next(Object.assign([], equipments));
  }

  private async getEquipmentsSignalR() : Promise<Array<EquipmentState>> {
    if(this.isConnected())
      return await this.connection.invoke("EquipmentStatus");
    else return new Array<EquipmentState>();
  }

  private getEquipmentsHttp() : void {
    this.http.get<Array<EquipmentState>>(`${environment.apiBaseUrl}/Equipments`).subscribe(equipments => {
      this.store.equipments = equipments;
      this._equipments.next(Object.assign({}, equipments));
    });
  }

  private isConnected(){
    return this.connection != undefined && this.connection.state === signalR.HubConnectionState.Connected;
  }
}

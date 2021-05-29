import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import DataRobot from 'src/models/DataRobot';
import EquipmentState from 'src/models/EquipmentState';
import MotorRobot from 'src/models/MotorRobot';
import SearchData from 'src/models/SearchData';

class FactoryModel {
  static defaultSearchData(): SearchData {
    let search = new SearchData();
    search.idEquipment = undefined;
    search.nbData = 20;
    search.timestampAfter = 0;
    return search;
  }
}

@Injectable({
  providedIn: 'root'
})
export class DatastoreEquipmentService {

  //Store
  private store: {
    idEquipment: string;
    searchDataRobot: SearchData;
    dataRobot: Array<DataRobot>;
    equipmentSelected: EquipmentState;
    nbDataPull: number;
  } = {
    idEquipment: undefined,
    searchDataRobot: FactoryModel.defaultSearchData(),
    dataRobot: new Array<DataRobot>(),
    equipmentSelected: undefined,
    nbDataPull: 30
  }

  //Behavior
  private _idEquipment = new BehaviorSubject<string>(this.store.idEquipment);
  private _searchDataRobot = new BehaviorSubject<SearchData>(this.store.searchDataRobot);
  private _equipmentSelected = new BehaviorSubject<EquipmentState>(this.store.equipmentSelected);
  private _dataRobot = new BehaviorSubject<Array<DataRobot>>(this.store.dataRobot);
  private _lastDataRobot = new BehaviorSubject<DataRobot>(undefined);
  private _nbDataPull = new BehaviorSubject<number>(this.store.nbDataPull);

  //Observable
  readonly idEquipment = this._idEquipment.asObservable();
  readonly searchDataRobot = this._searchDataRobot.asObservable();
  readonly dataRobot = this._dataRobot.asObservable();
  readonly equipmentSelected = this._equipmentSelected.asObservable();
  readonly lastDataRobot = this._lastDataRobot.asObservable();
  readonly nbDataPull = this._nbDataPull.asObservable();

  //Other
  private connection: signalR.HubConnection = undefined;
 

  constructor(private http: HttpClient) { }

  public async setEquipmentIdSelected(id: string){

    //Set current idEquipment
    this.store.idEquipment = id;
    this._idEquipment.next(id);

    //Build signalR connection and try to connect
    await this.buildConnection(id);

    //If we are connected try to get state of current equipment and set
    var equipment : EquipmentState = undefined;
    if(this.isConnected())
      equipment = await this.connection.invoke("EquipmentStatus");

    this.setEquipmentSelected(equipment);
  }

  private async buildConnection(id: string){

    //if we are already connected stop the latest connection
    if(this.isConnected()){
      await this.connection.stop();
      this.connection = undefined;
    }

    //dont connect if id is undefined
    if(id === undefined)
      return;

    //build new hub with idEquipment
    this.connection = new signalR.HubConnectionBuilder()
      .configureLogging(signalR.LogLevel.Information)
      .withUrl(`${environment.wsBaseUrl}/equipment?idEquipment=${id}`)
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

    this.connection.on("onChangeEquipment", (data) => this.setEquipmentSelected(data));
    this.connection.on("onDataEquipment", (data) => this._lastDataRobot.next(Object.assign({}, data)));

  }

  private async setEquipmentSelected(equipment: EquipmentState){

    if(equipment === null)
      equipment = undefined;

    //set the new equipment selected
    this.store.equipmentSelected = equipment;
    this._equipmentSelected.next(equipment);

    //get latest data of this equipment
    var data : Array<DataRobot> = new Array<DataRobot>();
    if(equipment != undefined){
      data = await this.connection.invoke("LastDataEquipment", this.store.nbDataPull);
    }
    //assign data of equipment
    this.store.dataRobot = data;
    this._dataRobot.next(Object.assign([], this.store.dataRobot));
  }

  private isConnected(){
    return this.connection != undefined && this.connection.state === signalR.HubConnectionState.Connected;
  }

  private getSearchDataLastEquipment(): void{
    this.http.post<Array<DataRobot>>(`${environment.apiBaseUrl}/Equipments/searchDataLast`, this.store.searchDataRobot).subscribe(data => {
      this.store.dataRobot = data;
      this._dataRobot.next(data);
    });
  }
}

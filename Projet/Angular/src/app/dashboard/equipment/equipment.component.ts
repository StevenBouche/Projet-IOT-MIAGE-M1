import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ChartDataSets, ChartOptions } from 'chart.js';
import EquipmentState from 'src/models/EquipmentState';
import DataRobot from 'src/models/DataRobot';
import { Subscription } from 'rxjs';
import { DatastoreEquipmentService } from 'src/services/store/equipment/datastore-equipment.service';
import { Color, Label } from 'ng2-charts';
import MotorRobot from 'src/models/MotorRobot';
import { DatastoreEquipmentStreamService } from 'src/services/store/equipmentStream/datastore-equipment-stream.service';

interface ValueTime{
  value: number;
  time: number;
}

interface IAdapterDataRobotValue{
  (data : DataRobot) : ValueTime;
}

interface ValueChart{
  dataSets: ChartDataSets[];
  labels: Label[],
  adapter: IAdapterDataRobotValue
}

@Component({
  selector: 'app-equipment',
  templateUrl: './equipment.component.html',
  styleUrls: ['./equipment.component.css']
})
export class EquipmentComponent implements OnInit, OnDestroy {

  //Chart Datas
  private maxSizeDataChart : number = 30;
  public chartTypeLigth: string = 'line';
  public chartLabelsLigth: Label[] = [];
  public chartDatasetsLigth: ChartDataSets[] = [ {data: [], label: 'Ligth'} ];
  public chartTypeTemperature: string = 'line';
  public chartLabelsTemperature: Label[] = [];
  public chartDatasetsTemperature: ChartDataSets[] = [ {data: [], label: 'Temperature'} ];
  public chartColors: Color[] = [ { borderColor: "#673ab7", pointBackgroundColor: "#673ab7" } ];
  public chartOptions: ChartOptions = {
    responsive: true,
    animation: { duration: 0 },
    legend: {
      labels: { fontColor: '#673ab7' }
    },
    scales: {
      yAxes: [{
        ticks: { fontColor: '#673ab7', min: 0 }
      }],
      xAxes: [{
        ticks: { fontColor: '#673ab7' }
      }]
    }
  };

  public chartOptionsMotor: ChartOptions = {
    responsive: true,
    animation: { duration: 0 },
    legend: {
      labels: { fontColor: '#673ab7' }
    },
    scales: {
      yAxes: [{
        ticks: { fontColor: '#673ab7', min: -100, max: 100 }
      }],
      xAxes: [{
        ticks: { fontColor: '#673ab7' }
      }]
    }
  };

  private storeCharts: {
    charts: Array<ValueChart>;
  } = {
    charts: new Array<ValueChart>(
      {
        dataSets: this.chartDatasetsLigth,
        labels: this.chartLabelsLigth,
        adapter: this.getValueTimeLigthOfDataRobot
      },
      {
        dataSets: this.chartDatasetsTemperature,
        labels: this.chartLabelsTemperature,
        adapter: this.getValueTimeTemperatureOfDataRobot
      }
    ),
  }

  public chartLabelsMotor: Label[] = ['MotorL', 'MotorR'];
  public chartDatasetsMotor: ChartDataSets[] = [ {
      data: [0, 0], 
      label: 'Motor',
      backgroundColor: [
        '#673ab7',
        '#673ab7'
      ],
      borderColor: [
        '#673ab7',
        '#673ab7'
      ],
    } 
  ];
  public chartTypeMotor: string = 'bar';

  //data component
  public equipment: EquipmentState;
  private dataRobot: Array<DataRobot>;
  public lastDataRobot: DataRobot;
  public lastDate: string;

  //data for stream
  public urlImage: "assets/FreeVector-No-Signal-TV.jpg";
  private objectURL = window.URL || window.webkitURL;
  private temporaryImage: string = "";
  private BASE64_MARKER = ';base64,';
  @ViewChild("imageStream", { static: true }) image: ElementRef;

  private dataRobot$ : Subscription;
  private lastDataRobot$ : Subscription;
  private equipmentSelected$ : Subscription;
  private nbDataPulled$ : Subscription;
  private dataVideo$ : Subscription;
  private dataMotor$ : Subscription;

  constructor(
    private service: DatastoreEquipmentService,
    private serviceStream: DatastoreEquipmentStreamService,
    private route: ActivatedRoute) { }

  async ngOnInit(): Promise<void> {

    //subscribe events dataStore
    this.nbDataPulled$ = this.service.nbDataPull.subscribe(nb => this.maxSizeDataChart = nb);
    this.equipmentSelected$ = this.service.equipmentSelected.subscribe(eq => this.equipment = eq);
    this.lastDataRobot$ = this.service.lastDataRobot.subscribe(data => this.setLastData(data, true));
    this.dataRobot$ = this.service.dataRobot.subscribe(datas => this.setDataRobot(datas));
    this.dataVideo$ = this.serviceStream.dataVideo.subscribe(item => this.preLoadImage(item));
    this.dataMotor$ = this.serviceStream.dataMotor.subscribe(item => this.setDataMotor(item));
    //get idEquipment from URL et set this ID in the datastore
    const idEquipment = this.route.snapshot.paramMap.get('id');
    await this.service.setEquipmentIdSelected(idEquipment);
    await this.serviceStream.setEquipmentIdSelected(idEquipment);
  }

  async ngOnDestroy(): Promise<void> {
    //unscribe to all events datastore and reset idEquipement
    if(this.nbDataPulled$) this.nbDataPulled$.unsubscribe();
    if(this.dataRobot$) this.dataRobot$.unsubscribe();
    if(this.lastDataRobot$) this.lastDataRobot$.unsubscribe();
    if(this.equipmentSelected$) this.equipmentSelected$.unsubscribe();
    if(this.dataVideo$) this.dataVideo$.unsubscribe();
    if(this.dataMotor$) this.dataMotor$.unsubscribe();
    await this.service.setEquipmentIdSelected(undefined);
    await this.serviceStream.setEquipmentIdSelected(undefined);
  }

  private setLastData(data: DataRobot, pushChart: boolean = false){
    this.lastDataRobot = data;
    if(this.lastDataRobot!=undefined)
      this.lastDate = new Date(this.lastDataRobot.timestamp*1000).toLocaleString()
    if(data != undefined && pushChart)
      this.pushDataRobotChart(data);
  }

  private setDataMotor(item: MotorRobot): void {

    var valueL = 0;
    var valueR = 0;

    if(item != undefined){
      valueL = item.MotorL;
      valueR = item.MotorR;
    }

    this.chartDatasetsMotor[0].data = [];
    this.chartDatasetsMotor[0].data.push(valueL);
    this.chartDatasetsMotor[0].data.push(valueR);
  }

  private setDataRobot(data: DataRobot[]){
    //set all data robot and push in charts
    this.dataRobot = data;
    this.dataRobot.forEach(data => this.pushDataRobotChart(data));
    //set the last data robot of this array
    if(data.length>0)
      this.setLastData(data[data.length-1]);
  }

  private pushDataRobotChart(data: DataRobot){
    this.storeCharts.charts.forEach(value => this.pushEventToChartData(value, data));
  }

  private pushEventToChartData(value: ValueChart, data: DataRobot): void {
    //test if have more data than maxSize charts
    if (value.dataSets[0].data.length >= this.maxSizeDataChart){
      value.dataSets[0].data.shift();
      value.labels.shift();
    }
    //get new value and push
    var valueTime : ValueTime = value.adapter(data);
    value.dataSets[0].data.push(valueTime.value);
    value.labels.push(new Date(valueTime.time*1000).toLocaleString());
  }
  
  private onLoadImage(img){
    img.onload = () => {
      var imageDiv = document.getElementById("containerStream");
      if(imageDiv == null || imageDiv == undefined) return;
      imageDiv.innerHTML = '';
      imageDiv.appendChild(img);
     }
  }

  private loadStreamOff(){
    var img = new Image();
    this.onLoadImage(img);
    // Set the new image
    img.src = 'assets/FreeVector-No-Signal-TV.jpg';
    img.className="image img-fluid";
  }

  private preLoadImage(url: string){

    //console.log(url);
    if(url === undefined){
      this.loadStreamOff();
      return;
    }
     
    var img = new Image();

    this.onLoadImage(img);

    // Destroy old image
    if(this.temporaryImage)
      this.objectURL.revokeObjectURL(this.temporaryImage);

    // Create a new image from binary data
    var imageDataBlob = this.convertDataURIToBlob(url);

    // Create a new object URL
    this.temporaryImage = this.objectURL.createObjectURL(imageDataBlob);

    // Set the new image
    img.src = this.temporaryImage;
    img.className="image img-fluid";
  }

  convertDataURIToBlob(dataURI: string) {
    // Validate input data
    if(!dataURI)
      return;

    // Convert image (in base64) to binary data
    var base64Index = dataURI.indexOf(this.BASE64_MARKER) + this.BASE64_MARKER.length;
    var base64 = dataURI.substring(base64Index);
    var raw = window.atob(base64);
    var rawLength = raw.length;
    var array = new Uint8Array(new ArrayBuffer(rawLength));

    for(var i = 0; i < rawLength; i++)
        array[i] = raw.charCodeAt(i);

    // Create and return a new blob object using binary data
    return new Blob([array], {type: "image/jpeg"});
  }

  getValueTimeTemperatureOfDataRobot(data: DataRobot) : ValueTime {
    return { value: data.temperature, time: data.timestamp }
  }

  getValueTimeLigthOfDataRobot(data: DataRobot) : ValueTime{
    return { value: data.ligth, time: data.timestamp }
  }

  getValuesTimesLigth() : ValueTime[]{
    return this.dataRobot.map(data => {
      return this.getValueTimeLigthOfDataRobot(data);
    }) as ValueTime[];
  }

  getValuesTimesTemperature() : ValueTime[]{
    return this.dataRobot.map(data => {
      return this.getValueTimeTemperatureOfDataRobot(data);
    }) as ValueTime[];
  }
}

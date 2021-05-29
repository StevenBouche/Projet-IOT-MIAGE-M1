import { NgModule } from '@angular/core';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { EquipmentComponent } from './equipment/equipment.component'
import { ChartsModule } from 'ng2-charts';
import { MatGridListModule } from '@angular/material/grid-list';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AgmCoreModule } from '@agm/core';
import { MDBBootstrapModule } from 'angular-bootstrap-md';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [
    EquipmentComponent
  ],
  imports: [
    CommonModule,
    DashboardRoutingModule,
    ChartsModule,
    MatGridListModule,
    FlexLayoutModule,
    AgmCoreModule.forRoot({
      apiKey: 'AIzaSyAvvoKmJ4g5ruaXM2dCEwiMNuQiIfMVuY0'
    }),
    MDBBootstrapModule.forRoot()
  ],
  exports: [

  ]
})
export class DashboardModule { }

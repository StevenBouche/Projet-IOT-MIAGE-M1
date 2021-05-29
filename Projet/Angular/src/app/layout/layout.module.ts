import { NgModule } from '@angular/core';
import { LayoutRoutingModule } from './layout-routing.module'
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatCardModule } from '@angular/material/card';

const materialModules = [
  MatIconModule,
  MatListModule,
  MatSidenavModule,
  MatToolbarModule,
  MatButtonModule,
  MatGridListModule,
  MatCardModule
];

@NgModule({
  declarations: [],
  imports: [
    ...materialModules,
    LayoutRoutingModule 
  ],
  exports: [
    ...materialModules
  ]
})
export class LayoutModule { }

import { Component, OnInit, OnDestroy} from '@angular/core';
import { Subscription } from 'rxjs';
import EquipmentState from 'src/models/EquipmentState';
import { DatastoreDashboardService } from 'src/services/store/dashboard/datastore-dashboard.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {

  public equipmentsState: Array<EquipmentState>;
  private subscription$: Subscription;
  
  constructor(private service: DatastoreDashboardService) { }

  ngOnInit(): void {
    this.subscription$ = this.service.equipments.subscribe(eq => this.equipmentsState = eq);
  }

  ngOnDestroy() {
    if(this.subscription$) this.subscription$.unsubscribe()
  }

}

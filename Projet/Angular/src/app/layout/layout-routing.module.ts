import { LayoutComponent } from './layout.component';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  
  {
    path: 'dashboard',
    component: LayoutComponent,
    loadChildren: () => import('./../dashboard/dashboard.module').then((m) => m.DashboardModule),
  },
  {
    path: 'dashboard/:id',
    component: LayoutComponent,
    loadChildren: () => import('./../dashboard/dashboard.module').then((m) => m.DashboardModule),
  },
  { path: '**', redirectTo: 'dashboard' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LayoutRoutingModule {}
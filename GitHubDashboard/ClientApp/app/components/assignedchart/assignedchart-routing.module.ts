import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AssignedChartComponent } from './assignedchart.component';

const assigned_chart: Routes = [
    { path: 'assignedchart/:owner/:repo', component: AssignedChartComponent },
];

@NgModule({
    imports: [
        RouterModule.forChild(assigned_chart)
    ],
    exports: [
        RouterModule
    ]
})
export class AssignedChartRoutingModule { }

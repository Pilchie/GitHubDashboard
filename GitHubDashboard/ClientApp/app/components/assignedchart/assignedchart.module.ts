import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AssignedChartComponent } from './assignedchart.component';
import { AssignedChartRoutingModule } from './assignedchart-routing.module';
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        AssignedChartRoutingModule
    ],
    declarations: [
        AssignedChartComponent
    ]
})
export class AssignedChartModule { }

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Chart } from 'chart.js';
import { ChartsModule } from 'ng2-charts';

import { AssignedChartComponent } from './assignedchart.component';
import { AssignedChartRoutingModule } from './assignedchart-routing.module';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ChartsModule,
        AssignedChartRoutingModule
    ],
    declarations: [
        AssignedChartComponent
    ]
})
export class AssignedChartModule { }

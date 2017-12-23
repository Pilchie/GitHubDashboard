import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QueryCountComponent } from './count.component';
import { CountRoutingModule } from './count-routing.module';
@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        CountRoutingModule
    ],
    declarations: [
        QueryCountComponent
    ]
})
export class CountModule { }

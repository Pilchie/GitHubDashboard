import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { QueryCountComponent } from './count.component';

const countRoutes: Routes = [
    { path: 'count/:owner/:repo', component: QueryCountComponent },
];

@NgModule({
    imports: [
        RouterModule.forChild(countRoutes)
    ],
    exports: [
        RouterModule
    ]
})
export class CountRoutingModule { }

import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute, Params } from '@angular/router';

@Component({
    selector: 'assignedchart',
    templateUrl: './assignedchart.component.html',
    styleUrls: ['./assignedchart.component.css']
})

export class AssignedChartComponent {
    public milestone?: string;

    public barChartOptions: any = {
        scaleShowVerticalLines: false,
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            yAxes: [{
                barPercentage: 0.5,
                categoryPercentage: 1.0,
            }],
            xAxes: [{
                ticks: {
                    beginAtZero: true,
                },
            }],
        },
    };
    public barChartType: string = 'horizontalBar';
    public barChartLegend: boolean = false;

    public barChartLabels: (string | undefined)[] = [];
    public barChartData?: any[];
    public barChartColors: any[] = [{ backgroundColor: "rgb(0, 156, 204)", hoverBackgroundColor: "rgb(0, 156, 204)", }];

    constructor(
        http: Http,
        route: ActivatedRoute,
        router: Router,
        @Inject('BASE_URL') baseUrl: string) {

        let owner: string = route.snapshot.params['owner'];
        let repo: string = route.snapshot.params['repo'];
        let milestone: string = route.snapshot.queryParams['milestone'];
        let labels: string = route.snapshot.queryParams['label'];
        http.get(baseUrl + `api/Query/AssignedChart/${owner}/${repo}/${milestone}/${labels}`).subscribe(result => {
            let data = result.json() as AssignedChartResult;
            if (data.assignees != null) {
                this.barChartLabels = data.assignees.map(a => a.assignee);
                this.barChartData = [{ data: data.assignees.map(a => a.count) }];
            }
            this.milestone = data.milestone == null ? "" : data.milestone;
        }, error => console.error(error));
    }
}

class AssigneeCount {
    public assignee?: string;
    public count?: number;
}

class AssignedChartResult {
    public milestone?: string;
    public assignees?: AssigneeCount[];
}

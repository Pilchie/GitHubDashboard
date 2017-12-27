import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute, Params } from '@angular/router';

@Component({
    selector: 'assignedchart',
    templateUrl: './assignedchart.component.html',
    styleUrls: ['./assignedchart.component.css']
})

export class AssignedChartComponent {
    public result: AssignedChartResult;

    constructor(
        http: Http,
        route: ActivatedRoute,
        router: Router,
        @Inject('BASE_URL') baseUrl: string) {

        let owner: string = route.snapshot.params['owner'];
        let repo: string = route.snapshot.params['repo'];
        let milestone: string = route.snapshot.queryParams['milestone'];
        http.get(baseUrl + `api/Query/AssignedChart/${owner}/${repo}/${milestone}`).subscribe(result => {
            this.result = result.json() as AssignedChartResult;
        }, error => console.error(error));
    }
}

class AssigneeCount {
    public assignee: string;
    public count: number;
}

class AssignedChartResult {
    public milestone: string;
    public assignees: AssigneeCount[];
}

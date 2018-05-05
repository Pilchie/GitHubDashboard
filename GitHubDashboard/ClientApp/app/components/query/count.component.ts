import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute, Params } from '@angular/router';

@Component({
    selector: 'count',
    templateUrl: './count.component.html',
    styleUrls: ['./count.component.css']
})

export class QueryCountComponent {
    public result?: QueryCountResult;

    constructor(
        http: Http,
        route: ActivatedRoute,
        router: Router,
        @Inject('BASE_URL') baseUrl: string) {

        let owner: string = route.snapshot.params['owner'];
        let repo: string = route.snapshot.params['repo'];
        let milestone: string = route.snapshot.queryParams['milestone'];
        let labels: string = route.snapshot.queryParams['label'];
        http.get(baseUrl + `api/Query/CountByMilestone/${owner}/${repo}/${milestone}/${labels}`).subscribe(result => {
            let count = result.json() as number;
            this.result = new QueryCountResult(owner, repo, count, milestone, labels);
        }, error => console.error(error));
    }
}

class QueryCountResult {
    public query: string = "";
    constructor(
        public owner: string,
        public repo: string,
        public count: number,
        public milestone: string,
        public labels: string) {

        if (milestone) {
            if (this.milestone == "none") {
                this.query = "no%3Amilestone ";
            }
            else {
                this.query = `milestone%3A${milestone} `;
            }
        }

        if (labels) {
            for (let label of labels.toString().split(",")) {
                this.query += `label%3A${label} `;
            }
        }
    }
}

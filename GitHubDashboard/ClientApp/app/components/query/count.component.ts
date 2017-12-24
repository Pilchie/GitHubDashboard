import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute, Params } from '@angular/router';

@Component({
    selector: 'count',
    templateUrl: './count.component.html',
    styleUrls: ['./count.component.css']
})

export class QueryCountComponent {
    public result: QueryCountResult;

    constructor(
        http: Http,
        route: ActivatedRoute,
        router: Router,
        @Inject('BASE_URL') baseUrl: string) {

        let owner: string = route.snapshot.params['owner'];
        let repo: string = route.snapshot.params['repo'];
        let milestone: string = route.snapshot.queryParams['milestone'];
        http.get(baseUrl + `api/Query/CountByMilestone/${owner}/${repo}/${milestone}`).subscribe(result => {
            let count = result.json() as number;
            this.result = new QueryCountResult(owner, repo, count, milestone);
        }, error => console.error(error));
    }
}

class QueryCountResult {
    public milestone_query: string;
    constructor(
        public owner: string,
        public repo: string,
        public count: number,
        public milestone: string) {
        if (this.milestone == "any") {
            this.milestone_query = "";
        } else if (this.milestone == "none") {
            this.milestone_query = "no%3Amilestone";
        } else {
            this.milestone_query = `milestone%3A${milestone}`;
        }
    }
}

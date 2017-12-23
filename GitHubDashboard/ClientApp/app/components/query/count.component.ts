import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Router, ActivatedRoute, Params } from '@angular/router';

@Component({
    selector: 'count',
    templateUrl: './count.component.html'
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
            this.result = new QueryCountResult(count, milestone);
        }, error => console.error(error));
    }
}

class QueryCountResult {
    constructor(
        public count: number,
        public milestone: string) { }
}

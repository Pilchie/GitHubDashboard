import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'count',
    templateUrl: './count.component.html'
})
export class QueryCountComponent {
    public count: number;

    constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
        http.get(baseUrl + 'api/Query/Count').subscribe(result => {
            this.count = result.json() as number;
        }, error => console.error(error));
    }
}

import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';

  constructor(private _http: HttpClient) { }

  ngOnInit(): void {
    this._http.get('https://localhost:5001/api/User').subscribe((response => {
      console.log(response);
    }), (error) => {
      console.log(error);
    });
  }
}

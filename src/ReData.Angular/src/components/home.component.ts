
import {Component, OnInit} from '@angular/core';
import {EditDatasourceFormComponent} from './edit-datasource-form';
import {KeycloakService} from 'keycloak-angular';
import { KeycloakProfile } from 'keycloak-js';

@Component({
  standalone: true,
  imports: [
    EditDatasourceFormComponent
  ],
  selector: 'app-home',
  template: `
    <div class="flex flex-col place-items-center">
      <p>Welcome to the ReData</p>
      <br/>
      <p>Demo of my Etl query system</p>
    </div>
    @if (isLoggedIn) {
      <button type="button" (click)="logout()">Log out</button>
    } @else {
      <button type="button" (click)="login()">Log in</button>
    }

      @if(userProfile) {
      <h2>User information</h2>
      <table>
          <tr >
            <th scope="row">Username</th>
            <td>{{ userProfile.username }}</td>
          </tr>
          <tr >
            <th scope="row">First name</th>
            <td>{{ userProfile.firstName }}</td>
          </tr>
          <tr >
            <th scope="row">First name</th>
            <td>{{ userProfile.lastName }}</td>
          </tr>
          <tr >
            <th scope="row">E-mail</th>
            <td>{{ userProfile.email }}</td>
          </tr>
          <tr>
            <th scope="row">E-mail verified</th>
            <td>{{ userProfile.emailVerified ? 'Yes' : 'No' }}</td>
          </tr>
      </table>
        }
    <app-edit-datasource-form></app-edit-datasource-form>
  `,
  styles: []
})
export class HomeComponent implements OnInit {

  public isLoggedIn = false;

  public userProfile: KeycloakProfile | null = null;

  constructor(private readonly keycloak: KeycloakService) {}

  public async ngOnInit() {
    this.isLoggedIn = await this.keycloak.isLoggedIn();

    if (this.isLoggedIn) {
      this.userProfile = await this.keycloak.loadUserProfile();
    }
  }

  public login() {
    this.keycloak.login();
  }

  public logout() {
    this.keycloak.logout();
  }
}

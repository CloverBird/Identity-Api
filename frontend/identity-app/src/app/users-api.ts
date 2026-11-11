import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';

export type UserStatus = 'PendingRegistration' | 'Active' | 'Blocked';

export interface IdentityUser {
  id: string;
  email: string;
  status: UserStatus;
  emailConfirmed: boolean;
  createdAt: string;
}

export interface IdentityUsersPage {
  items: IdentityUser[];
  page: number;
  pageSize: number;
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class UsersApi {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiBaseUrl}/admin/users`;

  list(page: number, pageSize: number) {
    return this.http.get<IdentityUsersPage>(this.url, { params: { page, pageSize } });
  }

  get(id: string) {
    return this.http.get<IdentityUser>(`${this.url}/${id}`);
  }

  create(email: string, password: string) {
    return this.http.post<IdentityUser>(this.url, { email, password });
  }

  setBlocked(id: string, blocked: boolean) {
    const action = blocked ? 'block' : 'unblock';
    return this.http.post<Pick<IdentityUser, 'id' | 'status'>>(`${this.url}/${id}/${action}`, null);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}

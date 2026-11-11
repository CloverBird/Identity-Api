import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { IdentityUser, IdentityUsersPage, UsersApi, UserStatus } from './users-api';

@Component({
  selector: 'app-root',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  private readonly api = inject(UsersApi);
  private readonly pageSize = 20;
  private readonly saving = signal(false);

  protected readonly usersPage = signal<IdentityUsersPage | null>(null);
  protected readonly loading = signal(false);
  protected readonly busy = computed(() => this.loading() || this.saving());
  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.usersPage()?.totalCount ?? 0) / this.pageSize)),
  );
  protected readonly listError = signal('');
  protected readonly formError = signal('');
  protected readonly details = signal<IdentityUser | null>(null);
  protected readonly detailsLoading = signal(false);
  protected readonly detailsError = signal('');

  protected readonly statusLabels: Record<UserStatus, string> = {
    Active: 'Active',
    Blocked: 'Blocked',
    PendingRegistration: 'Pending registration',
  };

  protected readonly createForm = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(128)]],
  });

  ngOnInit(): void {
    void this.loadUsers();
  }

  protected async loadUsers(page = this.usersPage()?.page ?? 1): Promise<void> {
    this.loading.set(true);
    this.listError.set('');

    try {
      let result = await firstValueFrom(this.api.list(page, this.pageSize));
      const lastPage = Math.max(1, Math.ceil(result.totalCount / result.pageSize));

      if (result.page > lastPage) {
        result = await firstValueFrom(this.api.list(lastPage, this.pageSize));
      }

      this.usersPage.set(result);
    } catch (error) {
      this.listError.set(errorMessage(error, 'Could not load users.'));
    } finally {
      this.loading.set(false);
    }
  }

  protected async createUser(): Promise<void> {
    if (this.busy()) return;

    this.formError.set('');
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const { email, password } = this.createForm.getRawValue();

    try {
      await firstValueFrom(this.api.create(email, password));
      this.createForm.reset();
      await this.loadUsers(1);
    } catch (error) {
      this.formError.set(errorMessage(error, 'Could not add the user.'));
    } finally {
      this.saving.set(false);
    }
  }

  protected async viewUser(id: string, dialog: HTMLDialogElement): Promise<void> {
    if (this.detailsLoading()) return;

    this.details.set(null);
    this.detailsError.set('');
    this.detailsLoading.set(true);
    dialog.showModal();

    try {
      this.details.set(await firstValueFrom(this.api.get(id)));
    } catch (error) {
      this.detailsError.set(errorMessage(error, 'Could not load user details.'));
    } finally {
      this.detailsLoading.set(false);
    }
  }

  protected async changeStatus(user: IdentityUser): Promise<void> {
    if (this.busy()) return;

    this.saving.set(true);
    this.listError.set('');

    try {
      const update = await firstValueFrom(this.api.setBlocked(user.id, user.status !== 'Blocked'));
      this.usersPage.update(
        (page) =>
          page && {
            ...page,
            items: page.items.map((item) =>
              item.id === update.id ? { ...item, status: update.status } : item,
            ),
          },
      );
    } catch (error) {
      this.listError.set(errorMessage(error, 'Could not change the user status.'));
    } finally {
      this.saving.set(false);
    }
  }

  protected async deleteUser(user: IdentityUser): Promise<void> {
    if (this.busy()) return;

    this.saving.set(true);
    this.listError.set('');

    try {
      await firstValueFrom(this.api.delete(user.id));
      this.usersPage.update(
        (page) =>
          page && {
            ...page,
            items: page.items.filter((item) => item.id !== user.id),
            totalCount: page.totalCount - 1,
          },
      );
      await this.loadUsers();
    } catch (error) {
      this.listError.set(errorMessage(error, 'Could not delete the user.'));
    } finally {
      this.saving.set(false);
    }
  }
}

function errorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) return fallback;
  if (error.status === 0) return 'Could not reach the Identity API.';

  const body: unknown = error.error;
  if (typeof body === 'string' && body.trim()) return body;
  if (body && typeof body === 'object') {
    const { errors, detail } = body as { errors?: Record<string, string[]>; detail?: string };
    return errors ? Object.values(errors).flat().join(' ') || fallback : detail || fallback;
  }

  return fallback;
}

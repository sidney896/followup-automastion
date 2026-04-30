import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

@Component({
  standalone: true,
  selector: 'app-simple-customer-action',
  imports: [CommonModule, FormsModule, RouterLink],
  styles: [`
    .shell {
      max-width: 760px;
      margin: 1.5rem auto;
      background: #fff;
      border: 1px solid #dbe6f6;
      border-radius: 16px;
      box-shadow: 0 10px 24px rgba(19, 66, 125, 0.08);
      overflow: hidden;
    }

    .head {
      padding: 1rem 1.2rem;
      border-bottom: 1px solid #e7edf8;
      background: linear-gradient(145deg, #f8fbff 0%, #eef6ff 100%);
    }

    .head h2 {
      margin: 0;
      font-size: 1.6rem;
      letter-spacing: -0.01em;
      color: #13233f;
    }

    .head p {
      margin: 0.45rem 0 0;
      color: #4c5d79;
    }

    .body {
      padding: 1.1rem;
    }

    .field {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      color: #1d3353;
      font-weight: 600;
      margin-bottom: 0.8rem;
    }

    input,
    textarea {
      width: 100%;
      border: 1px solid #cedcf1;
      border-radius: 10px;
      padding: 0.6rem;
      font-size: 1rem;
      color: #16233d;
    }

    textarea {
      min-height: 90px;
      resize: vertical;
    }

    .actions {
      display: flex;
      gap: 0.6rem;
      flex-wrap: wrap;
      margin-top: 0.45rem;
    }

    .btn-primary,
    .btn-secondary {
      border: 1px solid #2d67df;
      border-radius: 10px;
      font-weight: 700;
      padding: 0.7rem 0.95rem;
      cursor: pointer;
      text-decoration: none;
    }

    .btn-primary {
      color: #fff;
      background: linear-gradient(90deg, #275ede 0%, #2d73ff 100%);
    }

    .btn-secondary {
      color: #2b66df;
      background: #fff;
    }

    .status {
      margin-top: 0.75rem;
      color: #244f73;
      font-size: 0.95rem;
    }
  `],
  template: `
    <section class="shell">
      <div class="head">
        <h2>{{ title }}</h2>
        <p>{{ description }}</p>
      </div>

      <div class="body">
        <label class="field" *ngIf="mode === 'remind-later'">
          Days until reminder
          <input type="number" [(ngModel)]="days" min="1" />
        </label>

        <label class="field">
          Optional note
          <textarea [(ngModel)]="note"></textarea>
        </label>

        <div class="actions">
          <button class="btn-primary" (click)="submit()">Submit</button>
          <a class="btn-secondary" [routerLink]="['/r', token]">Back to reminder</a>
        </div>

        <div class="status">{{ status }}</div>
      </div>
    </section>
  `,
})
export class SimpleCustomerActionComponent {
  token = '';
  mode = '';
  title = '';
  description = '';
  note = '';
  days = 7;
  status = '';

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {
    this.token = this.route.snapshot.paramMap.get('trackingToken') ?? '';
    this.mode = this.route.snapshot.data['mode'] ?? '';

    if (this.mode === 'remind-later') {
      this.title = 'Remind Me Later';
      this.description = 'Set a lower-pressure reminder date.';
    } else if (this.mode === 'already-repaired') {
      this.title = 'Already Repaired';
      this.description = 'Tell us this work has already been completed.';
    } else {
      this.title = 'Stop Reminders';
      this.description = 'Stop further automated reminders for this follow-up.';
    }
  }

  submit(): void {
    if (this.mode === 'remind-later') {
      this.api.remindLater(this.token, this.days, this.note).subscribe({
        next: () => (this.status = `Reminder rescheduled for ${this.days} day(s).`),
        error: () => (this.status = 'Could not save remind-later preference.'),
      });
      return;
    }

    if (this.mode === 'already-repaired') {
      this.api.alreadyRepaired(this.token, this.note).subscribe({
        next: () => (this.status = 'Already repaired captured.'),
        error: () => (this.status = 'Could not submit already repaired action.'),
      });
      return;
    }

    this.api.stop(this.token, this.note).subscribe({
      next: () => (this.status = 'Reminders stopped.'),
      error: () => (this.status = 'Could not stop reminders.'),
    });
  }
}

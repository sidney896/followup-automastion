import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  standalone: true,
  selector: 'app-request-callback',
  imports: [CommonModule, FormsModule, RouterLink],
  styles: [`
    .shell {
      max-width: 760px;
      margin: 1.5rem auto;
      background: #fff;
      border: 1px solid #dbe7f6;
      border-radius: 16px;
      box-shadow: 0 12px 28px rgba(18, 66, 120, 0.08);
      overflow: hidden;
    }

    .head {
      padding: 1.1rem 1.3rem;
      border-bottom: 1px solid #e7eef8;
      background: linear-gradient(145deg, #f8fbff 0%, #eef5ff 100%);
    }

    .head h2 {
      margin: 0;
      font-size: 1.8rem;
      letter-spacing: -0.01em;
      color: #13233f;
    }

    .head p {
      margin: 0.4rem 0 0;
      color: #4a5d7b;
    }

    .body {
      padding: 1.2rem;
    }

    .summary {
      border: 1px solid #dfe9f7;
      border-radius: 12px;
      background: #fbfdff;
      padding: 0.85rem 0.95rem;
      margin-bottom: 1rem;
      color: #2c3d58;
      line-height: 1.45;
    }

    .grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 0.9rem;
      margin-bottom: 0.9rem;
    }

    .field {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      font-weight: 600;
      color: #1d3353;
    }

    .full {
      grid-column: 1 / -1;
    }

    input,
    textarea,
    select {
      width: 100%;
      border: 1px solid #cddcf2;
      border-radius: 10px;
      padding: 0.6rem 0.65rem;
      background: #fff;
      color: #13213a;
      font-size: 1rem;
    }

    textarea {
      min-height: 96px;
      resize: vertical;
    }

    .actions {
      display: flex;
      gap: 0.6rem;
      flex-wrap: wrap;
      margin-top: 0.5rem;
    }

    .btn-primary,
    .btn-secondary {
      border-radius: 10px;
      border: 1px solid #2e67df;
      padding: 0.7rem 0.95rem;
      font-weight: 700;
      font-size: 1.02rem;
      text-decoration: none;
      cursor: pointer;
    }

    .btn-primary {
      color: #fff;
      background: linear-gradient(90deg, #275fde 0%, #2d72ff 100%);
    }

    .btn-secondary {
      color: #2b64df;
      background: #fff;
    }

    .status {
      margin-top: 0.75rem;
      color: #234d72;
      font-size: 0.95rem;
    }

    @media (max-width: 700px) {
      .grid {
        grid-template-columns: 1fr;
      }
    }
  `],
  template: `
    <section class="shell">
      <div class="head">
        <h2>We will call you back</h2>
        <p>Tell us when is best and a member of our team will be in touch.</p>
      </div>

      <div class="body">
        <div class="summary">
          <strong>Reminder summary</strong><br />
          We’ll call you from Riverside Motors regarding your follow-up request.
        </div>

        <div class="grid">
          <label class="field">
            When should we call?
            <select [(ngModel)]="preferredTime">
              <option value="As soon as possible">As soon as possible</option>
              <option value="This morning">This morning</option>
              <option value="This afternoon">This afternoon</option>
              <option value="Tomorrow">Tomorrow</option>
              <option value="Choose a time">Choose a time</option>
            </select>
          </label>

          <label class="field">
            Phone number
            <input [(ngModel)]="phoneNumber" placeholder="+44..." />
          </label>

          <label class="field full">
            Optional note
            <textarea [(ngModel)]="note" placeholder="Add any context for the advisor"></textarea>
          </label>
        </div>

        <div class="actions">
          <button class="btn-primary" (click)="submit()">Send request</button>
          <a class="btn-secondary" [routerLink]="['/r', token]">Back to reminder</a>
        </div>

        <div class="status">{{ status }}</div>
      </div>
    </section>
  `,
})
export class RequestCallbackComponent {
  token = '';
  preferredTime = 'As soon as possible';
  phoneNumber = '';
  note = '';
  status = '';

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiService) {
    this.token = this.route.snapshot.paramMap.get('trackingToken') ?? '';
  }

  submit(): void {
    if (!this.phoneNumber.trim()) {
      this.status = 'Phone number is required.';
      return;
    }

    this.api
      .callback(this.token, {
        preferredTime: this.preferredTime,
        phoneNumber: this.phoneNumber,
        note: this.note,
      })
      .subscribe({
        next: () => (this.status = 'Callback request sent. A real advisor will contact you.'),
        error: () => (this.status = 'Failed to submit callback request.'),
      });
  }
}

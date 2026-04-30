import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { FollowUpRow } from '../../shared/models';

@Component({
  standalone: true,
  selector: 'app-eligible-reminders',
  imports: [CommonModule],
  styles: [`
    .ops-shell {
      background: #fff;
      border: 1px solid #d8e5f5;
      border-radius: 14px;
      padding: 1rem;
      box-shadow: 0 10px 24px rgba(19, 66, 124, 0.08);
    }

    .head {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.7rem;
      margin-bottom: 0.8rem;
    }

    h2 {
      margin: 0;
      font-size: 1.7rem;
      color: #14253e;
      letter-spacing: -0.01em;
    }

    .filters {
      color: #4a5f7b;
      font-size: 0.95rem;
      margin-bottom: 0.7rem;
    }

    .actions {
      display: flex;
      gap: 0.55rem;
      flex-wrap: wrap;
      margin-bottom: 0.65rem;
    }

    .btn {
      border: 1px solid #2d67df;
      border-radius: 9px;
      padding: 0.5rem 0.75rem;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: linear-gradient(90deg, #285fe0 0%, #2d72ff 100%);
      color: #fff;
    }

    .btn-secondary {
      background: #fff;
      color: #2b66df;
    }

    .status {
      color: #244f73;
      margin-bottom: 0.7rem;
      min-height: 1.2rem;
    }

    .grid-title {
      margin: 0.9rem 0 0.45rem;
      font-size: 1.25rem;
      color: #1c3458;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      background: #fff;
      border: 1px solid #dbe7f7;
      border-radius: 10px;
      overflow: hidden;
      margin-bottom: 0.9rem;
    }

    th,
    td {
      padding: 0.5rem;
      border-bottom: 1px solid #e7eef9;
      text-align: left;
      vertical-align: top;
      font-size: 0.96rem;
      color: #213a5e;
    }

    thead th {
      background: #f3f8ff;
      font-size: 0.84rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      color: #4f6280;
    }

    .tag {
      display: inline-block;
      padding: 0.18rem 0.45rem;
      border-radius: 6px;
      border: 1px solid #c0d6f2;
      background: #f5f9ff;
      font-size: 0.82rem;
    }

    a {
      color: #2b66df;
      word-break: break-all;
    }
  `],
  template: `
    <section class="ops-shell">
      <div class="head">
        <h2>Eligible reminders queue</h2>
      </div>

      <div class="filters">Filters: Due today | All channels | All advisors | All statuses</div>

      <div class="actions">
        <button class="btn btn-secondary" (click)="load()">Refresh queue</button>
        <button class="btn btn-primary" (click)="createReminders()" [disabled]="selectedIds.size === 0">Create reminders</button>
        <button class="btn btn-primary" (click)="sendMock()">Send selected (mock)</button>
        <button class="btn btn-secondary" (click)="loadOutbox()">Refresh outbox</button>
      </div>

      <div class="status">{{ status }}</div>

      <table>
        <thead>
          <tr>
            <th></th>
            <th>Customer</th>
            <th>Vehicle</th>
            <th>Follow-up type</th>
            <th>Due date</th>
            <th>Status</th>
            <th>Blocked reason</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let row of rows">
            <td>
              <input
                type="checkbox"
                [disabled]="row.eligibilityStatus !== 'eligible'"
                [checked]="selectedIds.has(row.followUpItemId)"
                (change)="toggle(row.followUpItemId, $any($event.target).checked)"
              />
            </td>
            <td>{{ row.customerName }}</td>
            <td>{{ row.vehicleRegistration }} ({{ row.vehicleMakeModel }})</td>
            <td>{{ row.eventType }}</td>
            <td>{{ row.dueDate | date: 'yyyy-MM-dd' }}</td>
            <td><span class="tag">{{ row.eligibilityStatus }}</span></td>
            <td>{{ row.blockedReason || '-' }}</td>
          </tr>
        </tbody>
      </table>

      <h3 class="grid-title">Outbox (customer links)</h3>
      <table>
        <thead>
          <tr>
            <th>Customer</th>
            <th>Channel</th>
            <th>Status</th>
            <th>Personalised URL</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let item of outbox">
            <td>{{ item.customerName }}</td>
            <td>{{ item.channel }}</td>
            <td>{{ item.status }}</td>
            <td><a [href]="item.personalisedUrl" target="_blank">{{ item.personalisedUrl }}</a></td>
          </tr>
        </tbody>
      </table>
    </section>
  `,
})
export class EligibleRemindersComponent {
  rows: FollowUpRow[] = [];
  outbox: any[] = [];
  status = '';
  selectedIds = new Set<string>();

  constructor(private readonly api: ApiService) {
    this.load();
    this.loadOutbox();
  }

  toggle(id: string, checked: boolean): void {
    checked ? this.selectedIds.add(id) : this.selectedIds.delete(id);
  }

  load(): void {
    this.api.getEligible().subscribe({
      next: (rows) => (this.rows = rows),
      error: () => (this.status = 'Failed to load eligible queue.'),
    });
  }

  createReminders(): void {
    this.api.createReminders([...this.selectedIds]).subscribe({
      next: (res) => {
        this.status = `Created ${res.createdCount} reminder message(s).`;
        this.loadOutbox();
      },
      error: () => (this.status = 'Failed to create reminders.'),
    });
  }

  sendMock(): void {
    this.api.sendMock().subscribe({
      next: (res) => {
        this.status = `Mock sent/updated: ${res.sent}`;
        this.loadOutbox();
      },
      error: () => (this.status = 'Failed to send mock reminders.'),
    });
  }

  loadOutbox(): void {
    this.api.getOutbox().subscribe({
      next: (rows) => (this.outbox = rows),
      error: () => (this.status = 'Failed to load outbox.'),
    });
  }
}

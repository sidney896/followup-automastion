import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { AdvisorActionRow } from '../../shared/models';

@Component({
  standalone: true,
  selector: 'app-advisor-actions',
  imports: [CommonModule, FormsModule],
  styles: [`
    .ops-shell {
      background: #fff;
      border: 1px solid #d8e4f5;
      border-radius: 14px;
      padding: 1rem;
      box-shadow: 0 10px 24px rgba(18, 67, 123, 0.08);
    }

    h2 {
      margin: 0 0 0.6rem;
      font-size: 1.7rem;
      color: #14253e;
      letter-spacing: -0.01em;
    }

    .tabs {
      display: flex;
      gap: 0.45rem;
      flex-wrap: wrap;
      margin-bottom: 0.65rem;
    }

    .tab {
      border: 1px solid #c4d8f3;
      border-radius: 8px;
      padding: 0.3rem 0.55rem;
      background: #f7fbff;
      color: #375277;
      font-size: 0.84rem;
      font-weight: 700;
    }

    .actions {
      margin-bottom: 0.6rem;
    }

    .btn {
      border: 1px solid #2d67df;
      border-radius: 9px;
      padding: 0.48rem 0.72rem;
      font-weight: 700;
      cursor: pointer;
    }

    .btn-primary {
      background: linear-gradient(90deg, #285fe0 0%, #2d72ff 100%);
      color: #fff;
    }

    .btn-secondary {
      background: #fff;
      color: #2b66de;
    }

    .status {
      color: #244f73;
      min-height: 1.2rem;
      margin-bottom: 0.65rem;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      border: 1px solid #dce8f8;
      border-radius: 10px;
      overflow: hidden;
    }

    th,
    td {
      padding: 0.5rem;
      border-bottom: 1px solid #e7effa;
      text-align: left;
      vertical-align: top;
      font-size: 0.93rem;
      color: #213b61;
    }

    thead th {
      background: #f3f8ff;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      font-size: 0.79rem;
      color: #516481;
    }

    .overdue {
      color: #a01f1f;
      font-weight: 800;
    }

    .stack {
      display: flex;
      gap: 0.4rem;
      flex-wrap: wrap;
      align-items: center;
    }

    input,
    select {
      border: 1px solid #cddcf2;
      border-radius: 8px;
      padding: 0.36rem 0.45rem;
      color: #1f3556;
      font-size: 0.92rem;
      min-width: 115px;
    }
  `],
  template: `
    <section class="ops-shell">
      <h2>Advisor action queue</h2>

      <div class="tabs">
        <span class="tab">Urgent</span>
        <span class="tab">New</span>
        <span class="tab">In progress</span>
        <span class="tab">Overdue</span>
        <span class="tab">Closed today</span>
      </div>

      <div class="actions"><button class="btn btn-secondary" (click)="load()">Refresh queue</button></div>
      <div class="status">{{ status }}</div>

      <table>
        <thead>
          <tr>
            <th>SLA deadline</th>
            <th>Customer</th>
            <th>Vehicle</th>
            <th>Request</th>
            <th>Status</th>
            <th>Assign advisor</th>
            <th>Close with outcome</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let action of rows">
            <td><span [class.overdue]="action.isOverdue">{{ action.slaDeadline | date: 'yyyy-MM-dd HH:mm' }}</span></td>
            <td>{{ action.customerName }}</td>
            <td>{{ action.vehicleRegistration }}</td>
            <td>{{ action.customerAction }}</td>
            <td>{{ action.status }}</td>
            <td>
              <div class="stack">
                <input #assignTo placeholder="advisor" />
                <button class="btn btn-secondary" (click)="assign(action, assignTo.value)">Assign</button>
              </div>
            </td>
            <td>
              <div class="stack">
                <select #outcome>
                  <option value="booked">booked</option>
                  <option value="declined">declined</option>
                  <option value="not_reached">not_reached</option>
                  <option value="already_repaired_elsewhere">already_repaired_elsewhere</option>
                  <option value="wrong_contact">wrong_contact</option>
                  <option value="vehicle_sold">vehicle_sold</option>
                  <option value="other">other</option>
                </select>
                <button class="btn btn-primary" (click)="close(action, outcome.value)">Close</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  `,
})
export class AdvisorActionsComponent {
  rows: AdvisorActionRow[] = [];
  status = '';

  constructor(private readonly api: ApiService) {
    this.load();
  }

  load(): void {
    this.api.getAdvisorActions().subscribe({
      next: (rows) => (this.rows = rows),
      error: () => (this.status = 'Failed to load advisor queue.'),
    });
  }

  assign(action: AdvisorActionRow, assignedTo: string): void {
    if (!assignedTo.trim()) {
      this.status = 'Assigned advisor is required.';
      return;
    }

    this.api.assignAction(action.actionId, assignedTo).subscribe({
      next: () => {
        this.status = `Assigned ${action.followUpItemId} to ${assignedTo}.`;
        this.load();
      },
      error: () => (this.status = 'Assign failed.'),
    });
  }

  close(action: AdvisorActionRow, outcome: string): void {
    this.api.closeAction(action.actionId, outcome, '').subscribe({
      next: () => {
        this.status = `Closed ${action.followUpItemId} with outcome ${outcome}.`;
        this.load();
      },
      error: () => (this.status = 'Close failed.'),
    });
  }
}

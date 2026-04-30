import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';

@Component({
  standalone: true,
  selector: 'app-service-team-activity',
  imports: [CommonModule],
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

    .actions {
      margin-bottom: 0.7rem;
    }

    .btn {
      border: 1px solid #2d67df;
      border-radius: 9px;
      background: #fff;
      color: #2b66de;
      padding: 0.48rem 0.75rem;
      font-weight: 700;
      cursor: pointer;
    }

    .kpis {
      display: grid;
      grid-template-columns: repeat(5, minmax(0, 1fr));
      gap: 0.55rem;
      margin-bottom: 0.75rem;
    }

    .kpi {
      border: 1px solid #d8e6f8;
      border-radius: 9px;
      padding: 0.6rem;
      background: #f8fbff;
    }

    .kpi .label {
      color: #50607b;
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }

    .kpi .value {
      color: #16345d;
      font-size: 1.2rem;
      font-weight: 800;
      margin-top: 0.22rem;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      border: 1px solid #dce7f7;
      border-radius: 10px;
      overflow: hidden;
    }

    th,
    td {
      padding: 0.5rem;
      border-bottom: 1px solid #e7eef9;
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

    @media (max-width: 980px) {
      .kpis {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }
  `],
  template: `
    <section class="ops-shell">
      <h2>Service Team Follow-Up Activity</h2>

      <div class="actions"><button class="btn" (click)="load()">Refresh activity</button></div>

      <div class="kpis">
        <div class="kpi"><div class="label">Messages sent</div><div class="value">{{ sentCount }}</div></div>
        <div class="kpi"><div class="label">Links opened</div><div class="value">{{ openedCount }}</div></div>
        <div class="kpi"><div class="label">Callbacks</div><div class="value">{{ callbackCount }}</div></div>
        <div class="kpi"><div class="label">Calls made</div><div class="value">{{ callCount }}</div></div>
        <div class="kpi"><div class="label">Open opportunities</div><div class="value">{{ openCount }}</div></div>
      </div>

      <table>
        <thead>
          <tr>
            <th>Customer</th>
            <th>Vehicle</th>
            <th>Type</th>
            <th>Channel</th>
            <th>Sent</th>
            <th>Opened</th>
            <th>Action</th>
            <th>Assigned advisor</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let row of rows">
            <td>{{ row.customerName }}</td>
            <td>{{ row.vehicleRegistration }}</td>
            <td>{{ row.eventType }}</td>
            <td>{{ row.channel || '-' }}</td>
            <td>{{ row.sentAt | date: 'yyyy-MM-dd HH:mm' }}</td>
            <td>{{ row.openedAt | date: 'yyyy-MM-dd HH:mm' }}</td>
            <td>{{ row.customerAction || '-' }}</td>
            <td>{{ row.assignedTo || '-' }}</td>
            <td>{{ row.status || '-' }}</td>
          </tr>
        </tbody>
      </table>
    </section>
  `,
})
export class ServiceTeamActivityComponent {
  rows: any[] = [];

  get sentCount(): number {
    return this.rows.filter((r) => !!r.sentAt).length;
  }

  get openedCount(): number {
    return this.rows.filter((r) => !!r.openedAt).length;
  }

  get callbackCount(): number {
    return this.rows.filter((r) => r.customerAction === 'callback_requested').length;
  }

  get callCount(): number {
    return this.rows.filter((r) => r.customerAction === 'call_dealer_clicked').length;
  }

  get openCount(): number {
    return this.rows.filter((r) => r.status && r.status !== 'closed').length;
  }

  constructor(private readonly api: ApiService) {
    this.load();
  }

  load(): void {
    this.api.getActivity().subscribe((rows) => (this.rows = rows));
  }
}

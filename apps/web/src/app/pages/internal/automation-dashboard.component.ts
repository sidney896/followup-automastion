import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  standalone: true,
  selector: 'app-automation-dashboard',
  imports: [CommonModule, RouterLink],
  styles: [`
    .ops-shell {
      background: #fff;
      border: 1px solid #d8e5f5;
      border-radius: 14px;
      padding: 1rem;
      box-shadow: 0 10px 24px rgba(20, 67, 123, 0.08);
    }

    .head {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 1rem;
      flex-wrap: wrap;
      margin-bottom: 0.9rem;
    }

    .head h2 {
      margin: 0;
      font-size: 1.85rem;
      color: #13233e;
      letter-spacing: -0.01em;
    }

    .sub {
      color: #42536e;
      margin-top: 0.3rem;
      font-size: 1rem;
    }

    .actions {
      display: flex;
      gap: 0.55rem;
      flex-wrap: wrap;
    }

    .btn {
      border: 1px solid #2d67df;
      border-radius: 10px;
      padding: 0.55rem 0.8rem;
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

    .kpis {
      display: grid;
      grid-template-columns: repeat(4, minmax(0, 1fr));
      gap: 0.65rem;
      margin: 0.8rem 0;
    }

    .kpi {
      border: 1px solid #d6e5f8;
      border-radius: 10px;
      background: #f9fbff;
      padding: 0.7rem;
    }

    .kpi .label {
      font-size: 0.85rem;
      color: #50617d;
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }

    .kpi .value {
      font-size: 1.4rem;
      color: #17345d;
      font-weight: 800;
      margin-top: 0.2rem;
    }

    .funnel {
      border: 1px solid #d8e5f6;
      border-radius: 10px;
      background: #fff;
      padding: 0.7rem;
      color: #2f425f;
      margin-bottom: 0.8rem;
    }

    .links {
      display: grid;
      grid-template-columns: repeat(4, minmax(0, 1fr));
      gap: 0.6rem;
    }

    .link {
      border: 1px solid #bdd4f1;
      border-radius: 10px;
      padding: 0.6rem;
      text-decoration: none;
      color: #1f4f8a;
      background: linear-gradient(145deg, #f9fcff 0%, #eef5ff 100%);
      font-weight: 700;
      text-align: center;
    }

    .status {
      color: #244f73;
      margin: 0.35rem 0 0.7rem;
      min-height: 1.2rem;
    }

    @media (max-width: 980px) {
      .kpis,
      .links {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }

    @media (max-width: 620px) {
      .kpis,
      .links {
        grid-template-columns: 1fr;
      }
    }
  `],
  template: `
    <section class="ops-shell">
      <div class="head">
        <div>
          <h2>Automated Follow-Up Dashboard</h2>
          <div class="sub">Site: Riverside Motors | Automation: ON</div>
        </div>

        <div class="actions">
          <button class="btn btn-primary" (click)="importData()">Import Mock Follow-Ups</button>
          <button class="btn btn-secondary" (click)="refresh()">Refresh KPI</button>
        </div>
      </div>

      <div class="status">{{ status }}</div>

      <div class="kpis" *ngIf="funnel">
        <div class="kpi"><div class="label">Due imported</div><div class="value">{{ funnel.imported }}</div></div>
        <div class="kpi"><div class="label">Eligible</div><div class="value">{{ funnel.eligible }}</div></div>
        <div class="kpi"><div class="label">Delivered</div><div class="value">{{ funnel.delivered }}</div></div>
        <div class="kpi"><div class="label">Callbacks</div><div class="value">{{ funnel.callbackRequested }}</div></div>
      </div>

      <div class="funnel" *ngIf="funnel">
        Funnel: Sent {{ funnel.sent || 0 }} → Opened {{ funnel.opened || 0 }} → Callback {{ funnel.callbackRequested || 0 }} → Booked {{ funnel.booked || 0 }}
      </div>

      <nav class="links">
        <a class="link" routerLink="/follow-up/automation/eligible">Eligible reminders</a>
        <a class="link" routerLink="/follow-up/automation/service-team-activity">Service team activity</a>
        <a class="link" routerLink="/follow-up/automation/advisor-actions">Advisor actions</a>
        <a class="link" routerLink="/follow-up/automation/reports">Reports</a>
      </nav>
    </section>
  `,
})
export class AutomationDashboardComponent {
  status = '';
  funnel: any;

  constructor(private readonly api: ApiService) {
    this.refresh();
  }

  importData(): void {
    this.api.importFollowUps('RIVER').subscribe({
      next: (res) => {
        this.status = `Imported ${res.importedCount} records (${res.correlationId}).`;
        this.refresh();
      },
      error: (err) => (this.status = `Import failed: ${err?.message ?? 'unknown error'}`),
    });
  }

  refresh(): void {
    this.api.getFunnel().subscribe({
      next: (data) => (this.funnel = data),
      error: () => (this.status = 'Could not load funnel report.'),
    });
  }
}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';

@Component({
  standalone: true,
  selector: 'app-reports',
  imports: [CommonModule],
  styles: [`
    .ops-shell {
      background: #fff;
      border: 1px solid #d8e4f5;
      border-radius: 14px;
      padding: 1rem;
      box-shadow: 0 10px 24px rgba(18, 67, 123, 0.08);
    }

    .head {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.75rem;
      flex-wrap: wrap;
      gap: 0.6rem;
    }

    .head h2 {
      margin: 0;
      font-size: 1.7rem;
      color: #14253e;
      letter-spacing: -0.01em;
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

    .cards {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 0.7rem;
    }

    .card {
      border: 1px solid #dbe6f7;
      border-radius: 10px;
      background: #f9fbff;
      padding: 0.7rem;
    }

    .card h3 {
      margin: 0 0 0.45rem;
      color: #1e3a60;
      font-size: 1.05rem;
    }

    pre {
      margin: 0;
      background: #fff;
      border: 1px solid #dce7f7;
      border-radius: 8px;
      padding: 0.6rem;
      white-space: pre-wrap;
      word-break: break-word;
      color: #253d60;
      font-size: 0.88rem;
      min-height: 120px;
    }

    @media (max-width: 880px) {
      .cards {
        grid-template-columns: 1fr;
      }
    }
  `],
  template: `
    <section class="ops-shell">
      <div class="head">
        <h2>Performance reports</h2>
        <button class="btn" (click)="load()">Refresh reports</button>
      </div>

      <div class="cards">
        <section class="card">
          <h3>Automation funnel</h3>
          <pre>{{ funnel | json }}</pre>
        </section>

        <section class="card">
          <h3>Blocked reasons</h3>
          <pre>{{ blockedReasons | json }}</pre>
        </section>

        <section class="card">
          <h3>Advisor SLA</h3>
          <pre>{{ sla | json }}</pre>
        </section>

        <section class="card">
          <h3>7-day opportunity</h3>
          <pre>{{ opportunity | json }}</pre>
        </section>
      </div>
    </section>
  `,
})
export class ReportsComponent {
  funnel: any;
  blockedReasons: any[] = [];
  sla: any;
  opportunity: any[] = [];

  constructor(private readonly api: ApiService) {
    this.load();
  }

  load(): void {
    this.api.getFunnel().subscribe((r) => (this.funnel = r));
    this.api.getBlockedReasons().subscribe((r) => (this.blockedReasons = r));
    this.api.getSla().subscribe((r) => (this.sla = r));
    this.api.getOpportunity7Day().subscribe((r) => (this.opportunity = r));
  }
}

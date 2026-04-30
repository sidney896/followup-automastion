import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';

@Component({
  standalone: true,
  selector: 'app-reminder-landing',
  imports: [CommonModule, RouterLink],
  styles: [`
    .reminder-shell {
      max-width: 860px;
      margin: 1.5rem auto;
      background: #fff;
      border: 1px solid #dce6f3;
      border-radius: 18px;
      overflow: hidden;
      box-shadow: 0 12px 30px rgba(17, 70, 130, 0.08);
      color: #16233a;
    }

    .top {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid #e4edf8;
    }

    .brand {
      font-size: 2rem;
      font-weight: 700;
      letter-spacing: -0.02em;
    }

    .trust {
      color: #5f6f89;
      font-weight: 500;
      font-size: 1.02rem;
    }

    .main {
      padding: 1.5rem;
      background: linear-gradient(180deg, #fbfdff 0%, #f6faff 100%);
    }

    .subhead {
      color: #2c66e3;
      text-align: center;
      font-weight: 700;
      margin-bottom: 0.35rem;
    }

    .headline {
      text-align: center;
      font-size: 2rem;
      letter-spacing: -0.02em;
      margin: 0 0 1.2rem;
    }

    .card {
      border: 1px solid #dfe9f6;
      border-radius: 14px;
      background: #fff;
      margin-bottom: 1rem;
      overflow: hidden;
    }

    .hero {
      display: grid;
      grid-template-columns: 1fr auto;
      gap: 1rem;
      align-items: center;
      background: linear-gradient(145deg, #f9fbff 0%, #eef4ff 100%);
      padding: 1.2rem;
    }

    .hero h3 {
      margin: 0;
      font-size: 2rem;
      letter-spacing: -0.02em;
    }

    .hero p {
      margin: 0.45rem 0;
      color: #3f516e;
      line-height: 1.45;
      font-size: 1.03rem;
    }

    .hero-icon {
      width: 132px;
      height: 132px;
      border-radius: 18px;
      border: 1px solid #cfddf6;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 2.5rem;
      color: #2e66df;
      background: rgba(255, 255, 255, 0.9);
    }

    .section-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-weight: 700;
      font-size: 1.8rem;
      padding: 0.95rem 1.1rem;
      border-bottom: 1px solid #e7eef8;
    }

    .rows {
      padding: 0.25rem 0.9rem 0.45rem;
    }

    .row {
      display: grid;
      grid-template-columns: 220px 1fr;
      gap: 1rem;
      padding: 0.75rem 0.2rem;
      border-bottom: 1px solid #edf2fa;
      align-items: center;
      font-size: 1.08rem;
    }

    .row:last-child {
      border-bottom: 0;
    }

    .label {
      color: #41516c;
      font-weight: 500;
    }

    .value {
      color: #13213a;
      font-weight: 600;
    }

    .value-badge {
      display: inline-block;
      padding: 0.35rem 0.7rem;
      border-radius: 10px;
      background: #edf4ff;
      border: 1px solid #d5e4fb;
      color: #2a67e5;
      font-weight: 700;
    }

    .helper {
      border: 1px solid #cfe0f8;
      border-radius: 12px;
      background: #f7fbff;
      color: #334761;
      padding: 0.9rem 1rem;
      margin: 0.8rem 0;
      font-size: 1.02rem;
    }

    .cta-primary,
    .cta-secondary {
      display: block;
      width: 100%;
      border-radius: 12px;
      border: 1px solid #2d67df;
      padding: 0.85rem 1rem;
      font-size: 1.45rem;
      font-weight: 700;
      text-align: center;
      cursor: pointer;
    }

    .cta-primary {
      color: #fff;
      background: linear-gradient(90deg, #2762df 0%, #2d71ff 100%);
      margin-bottom: 0.6rem;
      text-decoration: none;
      line-height: 1.2;
    }

    .cta-secondary {
      color: #2a67e1;
      background: #fff;
      margin: 0;
    }

    .links {
      display: flex;
      justify-content: space-between;
      gap: 0.75rem;
      flex-wrap: wrap;
      margin: 1rem 0 0.2rem;
      font-size: 1.04rem;
    }

    .links a {
      color: #2b66df;
      text-decoration: none;
      font-weight: 500;
    }

    .privacy {
      margin-top: 1rem;
      padding-top: 0.75rem;
      border-top: 1px solid #e4ecf7;
      color: #5b6f8f;
      text-align: center;
      font-size: 0.98rem;
    }

    .status {
      margin-top: 0.6rem;
      color: #214c71;
      font-size: 0.95rem;
    }

    @media (max-width: 760px) {
      .brand { font-size: 1.55rem; }
      .trust { display: none; }
      .headline { font-size: 1.5rem; }
      .hero { grid-template-columns: 1fr; }
      .hero-icon { width: 100%; height: 96px; }
      .row { grid-template-columns: 1fr; gap: 0.35rem; }
      .links { justify-content: flex-start; }
    }
  `],
  template: `
    <section class="reminder-shell" *ngIf="data">
      <div class="top">
        <div class="brand">{{ data?.dealer?.name || 'Dealer' }}</div>
        <div class="trust">Trusted care for your vehicle</div>
      </div>

      <div class="main" *ngIf="!data.expired; else expiredState">
        <div class="subhead">autoVHC Reminder</div>
        <h2 class="headline">A quick reminder about your vehicle</h2>

        <div class="card hero">
          <div>
            <h3>Hello {{ data.customerName }}</h3>
            <p>You asked us to remind you about a deferred repair on your vehicle.</p>
            <p>We're here to help when you're ready to get it sorted.</p>
          </div>
          <div class="hero-icon">✓</div>
        </div>

        <div class="card">
          <div class="section-title">What was deferred</div>
          <div class="rows">
            <div class="row">
              <div class="label">Deferred repair item</div>
              <div class="value"><span class="value-badge">{{ data.followUpDescription }}</span></div>
            </div>
            <div class="row">
              <div class="label">Vehicle</div>
              <div class="value">{{ data.vehicleMakeModel }}</div>
            </div>
            <div class="row">
              <div class="label">Registration</div>
              <div class="value">{{ data.vehicleRegistration }}</div>
            </div>
            <div class="row">
              <div class="label">Original inspection date</div>
              <div class="value">{{ data.originalInspectionDate || '-' }}</div>
            </div>
          </div>
        </div>

        <div class="card">
          <div class="section-title">Dealer contact</div>
          <div class="rows">
            <div class="row">
              <div class="label">Phone</div>
              <div class="value">{{ data.dealer.phone }}</div>
            </div>
            <div class="row">
              <div class="label">Email</div>
              <div class="value">{{ data.dealer.email }}</div>
            </div>
            <div class="row">
              <div class="label">Address</div>
              <div class="value">{{ data.dealer.address }}</div>
            </div>
            <div class="row">
              <div class="label">Opening hours</div>
              <div class="value">{{ data.dealer.openingHours }}</div>
            </div>
          </div>
        </div>

        <div class="helper">A real service advisor will be happy to help you arrange a convenient booking.</div>

        <a class="cta-primary" [routerLink]="['/r', token, 'callback']">Request callback</a>
        <button class="cta-secondary" (click)="callDealerClick()">Call dealer</button>

        <div class="links">
          <a [routerLink]="['/r', token, 'remind-later']">Remind me later</a>
          <a [routerLink]="['/r', token, 'already-repaired']">Already repaired</a>
          <a [routerLink]="['/r', token, 'stop']">Stop reminders</a>
        </div>

        <div class="privacy">Your data is secure and will only be used to help with your vehicle service.</div>
        <div class="status">{{ status }}</div>
      </div>

      <ng-template #expiredState>
        <div class="main">
          <h2 class="headline">This reminder link has expired</h2>
          <div class="helper">Please contact your dealer directly for support.</div>
          <div class="status">{{ status }}</div>
        </div>
      </ng-template>
    </section>
  `,
})
export class ReminderLandingComponent {
  token = '';
  data: any;
  status = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly api: ApiService,
    private readonly router: Router,
  ) {
    this.token = this.route.snapshot.paramMap.get('trackingToken') ?? '';
    this.load();
  }

  load(): void {
    this.api.getReminder(this.token).subscribe({
      next: (data) => (this.data = data),
      error: () => {
        this.status = 'Reminder could not be loaded.';
        this.router.navigateByUrl('/follow-up/automation');
      },
    });
  }

  callDealerClick(): void {
    this.api.callDealerClick(this.token).subscribe({
      next: () => {
        this.status = 'Call dealer selected.';
        const phone = this.data?.dealer?.phone ?? '';
        const normalized = String(phone).replace(/[^+\d]/g, '');
        if (normalized) {
          window.location.href = `tel:${normalized}`;
        }
      },
      error: () => (this.status = 'Failed to capture call dealer click.'),
    });
  }
}

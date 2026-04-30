import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-settings',
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
      margin: 0 0 0.5rem;
      font-size: 1.7rem;
      color: #14253e;
      letter-spacing: -0.01em;
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 0.7rem;
      margin-top: 0.8rem;
    }

    .card {
      border: 1px solid #dae6f7;
      border-radius: 10px;
      background: #f9fcff;
      padding: 0.75rem;
    }

    .card h3 {
      margin: 0 0 0.35rem;
      color: #1f3a5f;
      font-size: 1.05rem;
    }

    .card p {
      margin: 0;
      color: #425672;
      line-height: 1.45;
      font-size: 0.94rem;
    }

    @media (max-width: 860px) {
      .grid {
        grid-template-columns: 1fr;
      }
    }
  `],
  template: `
    <section class="ops-shell">
      <h2>Automation settings</h2>

      <div class="grid">
        <section class="card">
          <h3>Role model</h3>
          <p>Internal pages use role-stub access: advisor, manager and admin.</p>
        </section>

        <section class="card">
          <h3>Credential handling</h3>
          <p>All source endpoint and provider credentials remain backend-only. No client-side secrets.</p>
        </section>

        <section class="card">
          <h3>Messaging mode</h3>
          <p>Provider integration is mocked. Outbox links represent message send via the agreed channel.</p>
        </section>

        <section class="card">
          <h3>Write-back mode</h3>
          <p>Core follow-up write-back is deferred; outcomes are stored in module workflow state.</p>
        </section>
      </div>
    </section>
  `,
})
export class SettingsComponent {}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-templates',
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

    p {
      margin: 0.35rem 0;
      color: #40536f;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      border: 1px solid #dce7f7;
      border-radius: 10px;
      overflow: hidden;
      margin-top: 0.8rem;
    }

    th,
    td {
      padding: 0.55rem;
      border-bottom: 1px solid #e8eff9;
      text-align: left;
      color: #223c61;
      font-size: 0.93rem;
    }

    thead th {
      background: #f3f8ff;
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      color: #50647f;
    }
  `],
  template: `
    <section class="ops-shell">
      <h2>Template manager</h2>
      <p>Template governance supports draft, pending approval, approved and archived states.</p>
      <p>This prototype uses seeded templates for deferred repair, MOT reminder and service reminder families.</p>

      <table>
        <thead>
          <tr>
            <th>Template family</th>
            <th>Channels</th>
            <th>Languages</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>followup.deferred.*</td>
            <td>SMS, WhatsApp, Email (mocked)</td>
            <td>en-GB, fr-FR</td>
            <td>Approved baseline</td>
          </tr>
          <tr>
            <td>followup.mot.*</td>
            <td>SMS, Email (mocked)</td>
            <td>en-GB</td>
            <td>Approved baseline</td>
          </tr>
          <tr>
            <td>followup.service.*</td>
            <td>Email (mocked)</td>
            <td>en-GB</td>
            <td>Approved baseline</td>
          </tr>
        </tbody>
      </table>
    </section>
  `,
})
export class TemplatesComponent {}

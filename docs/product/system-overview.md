# System Overview

## Summary
The prototype is a standalone system-of-engagement for follow-up conversion. It reads follow-up source records through a mocked import adapter, applies eligibility/suppression rules, generates tracked reminder links, records customer CTA actions, creates advisor tasks, and publishes reporting metrics.

## Components
- Angular UI (`apps/web`)
  - Internal operational views: dashboard, eligible queue, service team activity, advisor actions, templates, settings, reports.
  - Public customer pages: reminder landing, callback request, remind-later, already-repaired, stop-reminders.
- ASP.NET Core API (`apps/api`)
  - Import, reminders, tracking, advisor workflow, reports.
  - CORS enabled for local review hosts.
- Postgres (`apps/db`)
  - Module-owned workflow state and reporting aggregates.

## Data flow
1. User triggers import from internal dashboard.
2. API mock source returns follow-up candidates.
3. API normalizes and stores records in canonical follow-up tables.
4. Eligible records generate reminder messages and tracking tokens.
5. Outbox exposes personalised URLs for message simulation.
6. Customer actions update tracking events and advisor queue.
7. Advisor actions close with mandatory outcomes.
8. Reporting endpoints aggregate funnel/SLA/opportunity metrics.

## Security boundaries
- No client-side credential storage.
- No PII in reminder token URLs.
- Provider and core integration behavior remains mocked in prototype.

## Non-goals
- Live channel provider delivery.
- Production-grade SSO/tenant hardening.
- Core system write-back.

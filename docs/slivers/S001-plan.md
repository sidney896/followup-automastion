# S001 Plan - End-to-End Automated Follow-Up Prototype Baseline

## Objective
Deliver a runnable prototype with Angular frontend, .NET API, Postgres schema/seed, mocked import and messaging flow, and customer/advisor lifecycle coverage.

## Acceptance checks
- Import endpoint stores snapshot and normalized follow-up rows.
- Eligible queue exposes eligible/blocked/suppressed status.
- Reminder create + send mock updates message lifecycle.
- Outbox exposes personalised URLs.
- Public token route supports callback/call/remind later/already repaired/stop actions.
- Advisor queue supports assign + close with required outcome.
- Reports return funnel, blocked reasons, SLA and 7-day opportunity.

## Evidence to capture
- API request/response traces for each required endpoint.
- UI screenshots of dashboard, eligible, activity, advisor, reports, and reminder route.
- DB query snapshots for message and advisor state transitions.

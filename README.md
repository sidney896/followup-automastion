# autoVHC Automated Follow-Up Prototype

Prototype stack:
- Angular frontend: `apps/web`
- ASP.NET Core API: `apps/api`
- Postgres schema/seed: `apps/db`

## What is implemented
- Mock follow-up import flow (`/api/followup/import`) with normalization and eligibility states.
- Reminder message creation and mock-send lifecycle.
- Outbox with personalised tracking URLs.
- Public customer routes through tokenized URL actions.
- Advisor action queue with assign and mandatory close outcome API.
- Reports: funnel, blocked reasons, SLA, 7-day opportunity.
- Role-stub frontend routes for advisor/manager/admin.

## Prerequisites
- .NET SDK 10+
- Node 22+
- PostgreSQL local server reachable on `localhost:5432`

## Local setup
1. Create database if needed:
```bash
createdb autovhc_prototype
```
2. Run API:
```bash
dotnet run --project apps/api
```
3. Install web dependencies and run Angular app:
```bash
cd apps/web
npm install
npm start
```
4. Open web UI: `http://localhost:4200`

The API bootstraps schema and seed automatically from:
- `apps/db/migrations/001_init.sql`
- `apps/db/seed/001_seed.sql`

## Key UI routes
- `/follow-up/automation`
- `/follow-up/automation/eligible`
- `/follow-up/automation/service-team-activity`
- `/follow-up/automation/advisor-actions`
- `/follow-up/automation/templates`
- `/follow-up/automation/settings`
- `/follow-up/automation/reports`

Public routes:
- `/r/:trackingToken`
- `/r/:trackingToken/callback`
- `/r/:trackingToken/remind-later`
- `/r/:trackingToken/already-repaired`
- `/r/:trackingToken/stop`

## Notes
- Messaging and core autoVHC write-back are mocked by design.
- No customer PII is placed in tracking URLs.
- This is a prototype workflow, not production-ready deployment.

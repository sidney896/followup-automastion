# autoVHC Automated Follow-Up Prototype

This repository contains a fully runnable prototype for automated follow-up workflows using Angular, ASP.NET Core, and Postgres.

## Repository layout
- `apps/web`: Angular UI and customer reminder pages.
- `apps/api`: ASP.NET Core workflow API, mock source adapter, reporting endpoints.
- `apps/db`: SQL migration and seed scripts.
- `docs/governance`: Working rules, coding guidance, and PR structure.
- `docs/product`: Product intent, capability boundaries, and architecture notes.
- `docs/slivers`: Incremental delivery plans.
- `docs/outputs`: Delivery evidence and completion notes.
- `docs/review`: Reviewer onboarding and navigation.

## Prototype status
Confirmed capability:
- Imports follow-up items from a mocked core source.
- Applies eligibility, suppression, and blocked reason logic.
- Generates personalised customer reminder links and tracks CTA events.
- Creates advisor actions with SLA and mandatory close outcomes.
- Exposes funnel/SLA/blocked/7-day opportunity reporting.

Out of scope:
- Live provider messaging integration.
- Production SSO integration.
- Core follow-up write-back to external systems.

## Run locally
Prerequisites:
- .NET SDK 10+
- Node 22+
- PostgreSQL listening on `localhost:5432`

Run API:
```bash
dotnet run --project apps/api --urls http://127.0.0.1:5010
```

Run web app:
```bash
cd apps/web
npm install
npm start -- --host 127.0.0.1 --port 4300
```

Open:
- Internal UI: `http://127.0.0.1:4300/follow-up/automation`
- API root: `http://127.0.0.1:5010/`

## Key routes
Internal:
- `/follow-up/automation`
- `/follow-up/automation/eligible`
- `/follow-up/automation/service-team-activity`
- `/follow-up/automation/advisor-actions`
- `/follow-up/automation/templates`
- `/follow-up/automation/settings`
- `/follow-up/automation/reports`

Customer:
- `/r/:trackingToken`
- `/r/:trackingToken/callback`
- `/r/:trackingToken/remind-later`
- `/r/:trackingToken/already-repaired`
- `/r/:trackingToken/stop`

## Reviewer quick start
1. Read `docs/review/review-start-here.md`.
2. Read the PR narrative in `docs/outputs/S001/pr-description.md`.
3. Read execution evidence in `docs/outputs/S001/completion-note.md`.
4. Validate acceptance criteria in `docs/slivers/S001-plan.md`.

## Security and data handling
- No live credentials are stored in this repository.
- API credentials and provider keys are backend-only by design.
- Tracking links use opaque tokens and avoid customer PII.

## Notes
This implementation is a prototype. It demonstrates workflow behavior and reviewable architecture, not production readiness.

# S001 Review Checklist

## Scope and behavior
- [ ] Import flow uses backend-only source adapter.
- [ ] Eligibility states include eligible/blocked/suppressed with reason.
- [ ] Outbox produces personalised reminder URLs.
- [ ] Customer CTAs update tracking and advisor workflow.
- [ ] Advisor close action requires outcome.

## API and data
- [ ] Canonical follow-up model fields are present and consistent.
- [ ] Tracking tokens are opaque and free of PII.
- [ ] Reporting endpoints return expected funnel/SLA/opportunity data.

## UI coverage
- [ ] Internal pages render and navigate correctly.
- [ ] Customer pages follow specification hierarchy and CTA order.
- [ ] Error statuses are visible and actionable.

## Documentation
- [ ] PR description includes changed/tested/out-of-scope/known-risk sections.
- [ ] Completion note includes checks run/passed/failed/follow-up.
- [ ] Reviewer start guide points to the right evidence files.

## Hygiene
- [ ] Repository has no uncommitted changes.
- [ ] No credentials committed.
- [ ] Folder structure aligns with repository working guidance.

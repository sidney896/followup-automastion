# Automated Follow-Up Prototype Scope

## Confirmed capability
- Imports follow-up records from a mocked source adapter through backend-only APIs.
- Evaluates eligibility and blocked/suppressed reasons.
- Creates reminder messages with opaque personalised tracking URLs.
- Supports customer CTA capture on token URLs.
- Creates and closes advisor actions with mandatory outcomes.
- Exposes funnel, blocked reason, SLA and 7-day opportunity reporting endpoints.

## Prototype constraints
- Messaging provider is mocked.
- Core write-back is not integrated.
- Internal auth is role stub only.
- Rules and schema are optimized for validation, not production hardening.

## Out of scope
- Live Infobip integration.
- Production SSO/OIDC.
- Live autoVHC API credentials.
- Marketing consent-country policy enforcement beyond baseline suppression behavior.

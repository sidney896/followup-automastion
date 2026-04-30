# Angular Coding Guidance

## UI structure
- Use standalone components with explicit imports.
- Keep route-level pages focused on one operational context.
- Prefer stable layout containers and clear table/card hierarchy.

## State and API usage
- Keep API calls in dedicated services.
- Keep component logic focused on presentation and user actions.
- Display explicit user-facing status for async failures.

## Styling
- Use consistent spacing, typography, and card layout across internal workflows.
- Keep mobile breakpoints explicit.
- Ensure CTA controls are full-width where mockups require it.

## Accessibility and clarity
- Keep visible labels for all form controls.
- Ensure action button intent is explicit in text.
- Avoid ambiguous icon-only actions for critical workflow steps.

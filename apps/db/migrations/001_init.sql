CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS dealer_site_config (
  site_code TEXT PRIMARY KEY,
  dealer_name TEXT NOT NULL,
  phone TEXT NOT NULL,
  email TEXT NOT NULL,
  address TEXT NOT NULL,
  opening_hours TEXT NOT NULL,
  default_language_code TEXT NOT NULL,
  timezone TEXT NOT NULL DEFAULT 'Europe/London'
);

CREATE TABLE IF NOT EXISTS followup_import_snapshot (
  snapshot_id UUID PRIMARY KEY,
  site_code TEXT NOT NULL,
  request_filters JSONB NOT NULL,
  imported_at TIMESTAMPTZ NOT NULL,
  record_count INT NOT NULL,
  correlation_id TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS followup_event (
  followup_item_id TEXT PRIMARY KEY,
  event_type TEXT NOT NULL,
  site_code TEXT NOT NULL REFERENCES dealer_site_config(site_code),
  customer_id TEXT,
  customer_name TEXT NOT NULL,
  mobile_number TEXT,
  email_address TEXT,
  preferred_channel TEXT,
  vehicle_registration TEXT NOT NULL,
  vehicle_make_model TEXT NOT NULL,
  followup_description TEXT NOT NULL,
  original_inspection_date DATE,
  due_date DATE NOT NULL,
  item_status TEXT NOT NULL,
  disable_follow_up BOOLEAN NOT NULL,
  estimated_value NUMERIC(12, 2),
  language_code TEXT,
  language_id INT,
  eligibility_status TEXT NOT NULL,
  blocked_reason TEXT,
  imported_at TIMESTAMPTZ NOT NULL,
  updated_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS reminder_template (
  template_id UUID PRIMARY KEY,
  template_family TEXT NOT NULL,
  event_type TEXT NOT NULL,
  channel TEXT NOT NULL,
  language_code TEXT NOT NULL,
  translation_key TEXT NOT NULL,
  body TEXT NOT NULL,
  approval_status TEXT NOT NULL,
  version INT NOT NULL
);

CREATE TABLE IF NOT EXISTS suppression_preference (
  suppression_id UUID PRIMARY KEY,
  customer_id TEXT NOT NULL,
  channel TEXT,
  opt_out_scope TEXT NOT NULL,
  source TEXT NOT NULL,
  reason TEXT,
  created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS reminder_message (
  message_id UUID PRIMARY KEY,
  followup_item_id TEXT NOT NULL REFERENCES followup_event(followup_item_id),
  event_type TEXT NOT NULL,
  provider TEXT NOT NULL,
  provider_message_id TEXT,
  channel TEXT NOT NULL,
  template_key TEXT NOT NULL,
  template_version INT NOT NULL,
  language_code TEXT NOT NULL,
  tracking_token TEXT NOT NULL UNIQUE,
  status TEXT NOT NULL,
  sent_at TIMESTAMPTZ,
  delivered_at TIMESTAMPTZ,
  opened_at TIMESTAMPTZ,
  clicked_at TIMESTAMPTZ,
  expires_at TIMESTAMPTZ NOT NULL,
  created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS tracking_event (
  event_id UUID PRIMARY KEY,
  message_id UUID NOT NULL REFERENCES reminder_message(message_id),
  event_type TEXT NOT NULL,
  event_timestamp TIMESTAMPTZ NOT NULL,
  details JSONB,
  user_agent TEXT
);

CREATE TABLE IF NOT EXISTS advisor_action (
  action_id UUID PRIMARY KEY,
  followup_item_id TEXT NOT NULL REFERENCES followup_event(followup_item_id),
  customer_action TEXT NOT NULL,
  status TEXT NOT NULL,
  assigned_to TEXT,
  callback_requested_at TIMESTAMPTZ,
  call_made_at TIMESTAMPTZ,
  sla_deadline TIMESTAMPTZ NOT NULL,
  outcome TEXT,
  notes TEXT,
  closed_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS idx_followup_due ON followup_event(due_date, eligibility_status);
CREATE INDEX IF NOT EXISTS idx_reminder_token ON reminder_message(tracking_token);
CREATE INDEX IF NOT EXISTS idx_advisor_status_sla ON advisor_action(status, sla_deadline);

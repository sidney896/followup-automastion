INSERT INTO dealer_site_config(site_code, dealer_name, phone, email, address, opening_hours, default_language_code, timezone)
VALUES
  ('RIVER', 'Riverside Motors', '020 7946 1200', 'service@riversidemotors.co.uk', '21 Station Road, Reading', 'Mon-Fri 08:00-18:00, Sat 08:00-13:00', 'en-GB', 'Europe/London'),
  ('NORTH', 'Northgate Service Centre', '0161 555 0171', 'service@northgate.example', '12 Bridge Street, Manchester', 'Mon-Fri 08:00-17:30', 'en-GB', 'Europe/London')
ON CONFLICT (site_code) DO UPDATE
SET dealer_name = EXCLUDED.dealer_name,
    phone = EXCLUDED.phone,
    email = EXCLUDED.email,
    address = EXCLUDED.address,
    opening_hours = EXCLUDED.opening_hours,
    default_language_code = EXCLUDED.default_language_code,
    timezone = EXCLUDED.timezone;

INSERT INTO reminder_template(template_id, template_family, event_type, channel, language_code, translation_key, body, approval_status, version)
VALUES
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'followup.deferred', 'deferred_repair', 'sms', 'en-GB', 'followup.deferred.sms.en-gb.v1', 'Hi {{customerName}}, you asked us to remind you about {{item}} on {{vehicleRegistration}}. Open: {{url}}', 'approved', 1),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'followup.mot', 'mot_reminder', 'sms', 'en-GB', 'followup.mot.sms.en-gb.v1', 'Hi {{customerName}}, your MOT reminder is due soon for {{vehicleRegistration}}. Open: {{url}}', 'approved', 1),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'followup.service', 'service_reminder', 'email', 'en-GB', 'followup.service.email.en-gb.v1', 'Service reminder for {{vehicleRegistration}}. Open: {{url}}', 'approved', 1),
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 'followup.deferred', 'deferred_repair', 'sms', 'fr-FR', 'followup.deferred.sms.fr-fr.v1', 'Bonjour {{customerName}}, rappel de suivi pour {{vehicleRegistration}}. Ouvrir: {{url}}', 'approved', 1)
ON CONFLICT (template_id) DO NOTHING;

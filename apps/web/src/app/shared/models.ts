export interface FollowUpRow {
  followUpItemId: string;
  eventType: string;
  siteCode: string;
  customerName: string;
  vehicleRegistration: string;
  vehicleMakeModel: string;
  followUpDescription: string;
  dueDate: string;
  preferredChannel?: string;
  eligibilityStatus: string;
  blockedReason?: string;
  itemStatus: string;
  disableFollowUp: boolean;
  estimatedValue?: number;
}

export interface AdvisorActionRow {
  actionId: string;
  followUpItemId: string;
  customerAction: string;
  status: string;
  assignedTo?: string;
  callbackRequestedAt?: string;
  callMadeAt?: string;
  slaDeadline: string;
  isOverdue: boolean;
  outcome?: string;
  customerName: string;
  vehicleRegistration: string;
  followUpDescription: string;
}

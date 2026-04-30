import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AdvisorActionRow, FollowUpRow } from '../shared/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = this.getBaseUrl();

  constructor(private readonly http: HttpClient) {}

  private getBaseUrl(): string {
    const host = typeof window !== 'undefined' && window.location.hostname === '127.0.0.1' ? '127.0.0.1' : 'localhost';
    return `http://${host}:5010`;
  }

  importFollowUps(siteCode = 'RIVER'): Observable<any> {
    return this.http.post(`${this.base}/api/followup/import`, {
      siteCode,
      recordLimit: 100,
      offset: 0,
      order: 'due_date asc',
      languageId: 1,
    });
  }

  getEligible(): Observable<FollowUpRow[]> {
    return this.http.get<FollowUpRow[]>(`${this.base}/api/followup/eligible`);
  }

  getActivity(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/api/followup/activity`);
  }

  createReminders(followUpItemIds: string[]): Observable<any> {
    return this.http.post(`${this.base}/api/reminders/create`, { followUpItemIds });
  }

  sendMock(messageIds: string[] = []): Observable<any> {
    return this.http.post(`${this.base}/api/reminders/send-mock`, { messageIds });
  }

  getOutbox(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/api/reminders/outbox`);
  }

  getAdvisorActions(): Observable<AdvisorActionRow[]> {
    return this.http.get<AdvisorActionRow[]>(`${this.base}/api/advisor-actions`);
  }

  assignAction(actionId: string, assignedTo: string): Observable<any> {
    return this.http.post(`${this.base}/api/advisor-actions/${actionId}/assign`, { assignedTo });
  }

  closeAction(actionId: string, outcome: string, notes: string, stopFurtherReminders = false): Observable<any> {
    return this.http.post(`${this.base}/api/advisor-actions/${actionId}/close`, {
      outcome,
      notes,
      stopFurtherReminders,
    });
  }

  getFunnel(): Observable<any> {
    return this.http.get(`${this.base}/api/reports/funnel`);
  }

  getBlockedReasons(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/api/reports/blocked-reasons`);
  }

  getSla(): Observable<any> {
    return this.http.get(`${this.base}/api/reports/sla`);
  }

  getOpportunity7Day(): Observable<any[]> {
    return this.http.get<any[]>(`${this.base}/api/reports/opportunity-7day`);
  }

  getReminder(token: string): Observable<any> {
    return this.http.get(`${this.base}/r/${token}`);
  }

  callback(token: string, payload: { preferredTime: string; phoneNumber: string; note?: string }): Observable<any> {
    return this.http.post(`${this.base}/r/${token}/callback`, payload);
  }

  callDealerClick(token: string): Observable<any> {
    return this.http.post(`${this.base}/r/${token}/call-dealer-click`, {});
  }

  remindLater(token: string, days: number, note?: string): Observable<any> {
    return this.http.post(`${this.base}/r/${token}/remind-later`, { days, note });
  }

  alreadyRepaired(token: string, note?: string): Observable<any> {
    return this.http.post(`${this.base}/r/${token}/already-repaired`, { note });
  }

  stop(token: string, reason?: string): Observable<any> {
    return this.http.post(`${this.base}/r/${token}/stop`, { reason });
  }
}

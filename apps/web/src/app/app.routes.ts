import { Routes } from '@angular/router';
import { roleGuard } from './role.guard';
import { AutomationDashboardComponent } from './pages/internal/automation-dashboard.component';
import { EligibleRemindersComponent } from './pages/internal/eligible-reminders.component';
import { ServiceTeamActivityComponent } from './pages/internal/service-team-activity.component';
import { AdvisorActionsComponent } from './pages/internal/advisor-actions.component';
import { TemplatesComponent } from './pages/internal/templates.component';
import { SettingsComponent } from './pages/internal/settings.component';
import { ReportsComponent } from './pages/internal/reports.component';
import { ReminderLandingComponent } from './pages/public/reminder-landing.component';
import { RequestCallbackComponent } from './pages/public/request-callback.component';
import { SimpleCustomerActionComponent } from './pages/public/simple-customer-action.component';

export const routes: Routes = [
  {
    path: 'follow-up/automation',
    canActivate: [roleGuard],
    data: { roles: ['advisor', 'manager', 'admin'] },
    component: AutomationDashboardComponent,
  },
  {
    path: 'follow-up/automation/eligible',
    canActivate: [roleGuard],
    data: { roles: ['advisor', 'manager', 'admin'] },
    component: EligibleRemindersComponent,
  },
  {
    path: 'follow-up/automation/service-team-activity',
    canActivate: [roleGuard],
    data: { roles: ['advisor', 'manager', 'admin'] },
    component: ServiceTeamActivityComponent,
  },
  {
    path: 'follow-up/automation/advisor-actions',
    canActivate: [roleGuard],
    data: { roles: ['advisor', 'manager', 'admin'] },
    component: AdvisorActionsComponent,
  },
  {
    path: 'follow-up/automation/templates',
    canActivate: [roleGuard],
    data: { roles: ['manager', 'admin'] },
    component: TemplatesComponent,
  },
  {
    path: 'follow-up/automation/settings',
    canActivate: [roleGuard],
    data: { roles: ['manager', 'admin'] },
    component: SettingsComponent,
  },
  {
    path: 'follow-up/automation/reports',
    canActivate: [roleGuard],
    data: { roles: ['advisor', 'manager', 'admin'] },
    component: ReportsComponent,
  },
  { path: 'r/:trackingToken', component: ReminderLandingComponent },
  { path: 'r/:trackingToken/callback', component: RequestCallbackComponent },
  { path: 'r/:trackingToken/remind-later', component: SimpleCustomerActionComponent, data: { mode: 'remind-later' } },
  { path: 'r/:trackingToken/already-repaired', component: SimpleCustomerActionComponent, data: { mode: 'already-repaired' } },
  { path: 'r/:trackingToken/stop', component: SimpleCustomerActionComponent, data: { mode: 'stop' } },
  { path: '', pathMatch: 'full', redirectTo: 'follow-up/automation' },
  { path: '**', redirectTo: 'follow-up/automation' },
];

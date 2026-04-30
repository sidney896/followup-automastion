import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  role = this.readRole();

  private readRole(): string {
    try {
      const value = localStorage.getItem('prototypeRole');
      return value ?? 'manager';
    } catch {
      return 'manager';
    }
  }

  setRole(value: string): void {
    this.role = value;
    try {
      localStorage.setItem('prototypeRole', value);
    } catch {
      // Ignore storage errors in restrictive browser contexts.
    }
  }
}

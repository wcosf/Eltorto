import { Injectable } from '@angular/core';

const STORAGE_KEY = 'admin_state';

interface TableState {
  pageIndex: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminStateService {
  private get storage(): Record<string, any> {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : {};
    } catch {
      return {};
    }
  }

  private set storage(data: Record<string, any>) {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(data));
    } catch {
    }
  }

  get lastVisitedRoute(): string | null {
    return this.storage['lastVisitedRoute'] ?? null;
  }

  set lastVisitedRoute(value: string | null) {
    const data = this.storage;
    if (value === null) {
      delete data['lastVisitedRoute'];
    } else {
      data['lastVisitedRoute'] = value;
    }
    this.storage = data;
  }

  getTableState(key: string): TableState | null {
    const data = this.storage;
    const states = data['tableStates'] ?? {};
    return states[key] ?? null;
  }

  saveTableState(key: string, state: TableState): void {
    const data = this.storage;
    const states = data['tableStates'] ?? {};
    states[key] = state;
    data['tableStates'] = states;
    this.storage = data;
  }
}

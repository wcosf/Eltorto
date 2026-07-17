import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface RecentAction {
  id: string;
  type: 'create' | 'update' | 'delete';
  entityType: string;
  entityId: number;
  entityName: string;
  timestamp: Date;
  link: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class RecentActionsService {
  private maxActions = 5;
  private actionsSubject = new BehaviorSubject<RecentAction[]>([]);
  public actions$ = this.actionsSubject.asObservable();

  constructor() {
    this.loadFromStorage();
  }

  addAction(action: Omit<RecentAction, 'id' | 'timestamp'>): void {
    const newAction: RecentAction = {
      ...action,
      id: Date.now().toString(36) + Math.random().toString(36).substr(2, 5),
      timestamp: new Date(),
    };
    const current = this.actionsSubject.value;
    const updated = [newAction, ...current].slice(0, this.maxActions);
    this.actionsSubject.next(updated);
    this.saveToStorage(updated);
  }

  clear(): void {
    this.actionsSubject.next([]);
    localStorage.removeItem('recentActions');
  }

  private saveToStorage(actions: RecentAction[]): void {
    try {
      localStorage.setItem('recentActions', JSON.stringify(actions));
    } catch {}
  }

  private loadFromStorage(): void {
    try {
      const data = localStorage.getItem('recentActions');
      if (data) {
        const parsed = JSON.parse(data);
        const actions = parsed.map((a: any) => ({
          ...a,
          timestamp: new Date(a.timestamp),
        }));
        this.actionsSubject.next(actions);
      }
    } catch {}
  }
}

import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RecentActionsService, RecentAction } from '../../../../core/recent-actions.service';

@Component({
  selector: 'app-recent-actions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './recent-actions.component.html',
  styleUrls: ['./recent-actions.component.scss']
})
export class RecentActionsComponent implements OnInit {
  @Output() actionClick = new EventEmitter<RecentAction>();
  actions: RecentAction[] = [];

  constructor(private recentActions: RecentActionsService) {}

  ngOnInit(): void {
    this.recentActions.actions$.subscribe(actions => {
      this.actions = actions;
    });
  }

  onActionClick(action: RecentAction): void {
    if (action.type !== 'delete') {
      this.actionClick.emit(action);
    }
  }

  getActionMessage(action: RecentAction): string {
    const name = action.link
      ? `<strong>${action.entityName}</strong>`
      : action.entityName;
    switch (action.type) {
      case 'create': return `Создан ${action.entityType} ${name}`;
      case 'update': return `Обновлён ${action.entityType} ${name}`;
      case 'delete': return `Удалён ${action.entityType} «${action.entityName}»`;
      default: return '';
    }
  }
}

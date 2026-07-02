export interface ConfirmationDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  confirmColor?: 'primary' | 'accent' | 'warn';
}

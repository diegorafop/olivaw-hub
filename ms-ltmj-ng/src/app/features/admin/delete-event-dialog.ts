import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
@Component({
  selector: 'app-delete-event-dialog',
  imports: [MatDialogModule, MatButtonModule],
  template: `<h2 mat-dialog-title>Excluir evento definitivamente?</h2>
    <mat-dialog-content
      >O evento “{{ title }}” e suas fotos serão apagados. Esta ação não pode ser
      desfeita.</mat-dialog-content
    >
    <mat-dialog-actions align="end">
      <button mat-button [mat-dialog-close]="false" cdkFocusInitial>Cancelar</button>
      <button mat-flat-button [mat-dialog-close]="true">Excluir definitivamente</button>
    </mat-dialog-actions>`,
})
export class DeleteEventDialog {
  readonly title = inject<string>(MAT_DIALOG_DATA);
}

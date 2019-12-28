import { Injectable, Component } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { MemberEditComponent } from '../members/member-list/member-edit/member-edit.component';

@Injectable()
export class PreventUnsavedChangesGuard implements CanDeactivate<MemberEditComponent> {
  canDeactivate(component: MemberEditComponent) {
    if (component.editForm.dirty) {
      return confirm('Are you sure you want to continue? Any unsaved changes will be lost');
    }

    return true;
  }
}

import { Injectable } from '@angular/core';
import { MemberEditComponent } from '../member/member-edit/member-edit.component';
import { CanDeactivate } from '@angular/router';
import { AlertifyService } from '../_services/alertify.service';

@Injectable()
export class PreventUnsavedChanges
  implements CanDeactivate<MemberEditComponent> {
  constructor(private alertify: AlertifyService) {}
  canDeactivate(component: MemberEditComponent) {
    if (component.editForm.dirty) {
      // this.alertify.confirm('Are you sure you want to continue ? Any unsaved changes will be lost!', this.justReturnTrue);
      return confirm(
        'Are you sure you want to continue ? Any unsaved changes will be lost'
      );
    }
    return true;
  }

  justReturnTrue(){
    return false;
  }
}

angular.module('eavEditTemplates', []).run(['$templateCache', function($templateCache) {
  'use strict';

  $templateCache.put('fields/boolean/boolean-default.html',
    "<div class=\"checkbox checkbox-labeled\"><switch class=\"tosic-green pull-left\" ng-model=value.Value></switch><div ng-include=\"'wrappers/eav-label.html'\"></div></div>"
  );


  $templateCache.put('fields/custom/custom-default.html',
    "<div class=\"alert alert-danger\">ERROR - This is a custom field, you shouldn't see this. You only see this because the custom-dialog is missing.</div><input class=\"form-control input-lg\" ng-pattern=vm.regexPattern ng-model=value.Value>"
  );


  $templateCache.put('fields/empty/empty-default.html',
    "<span></span>"
  );


  $templateCache.put('fields/entity/entity-default.html',
    "<div class=eav-entityselect><div ui-tree=options data-empty-placeholder-enabled=false><ol ui-tree-nodes ng-model=chosenEntities><li ng-repeat=\"item in chosenEntities track by $index\" ui-tree-node class=eav-entityselect-item><div ui-tree-handle><i icon=move title=\"{{ 'FieldType.Entity.DragMove' | translate }}\" class=\"pull-left eav-entityselect-sort\" ng-show=to.settings.Entity.AllowMultiValue></i> <span title=\"{{getEntityText(item) + ' (' + item + ')'}}\">{{getEntityText(item)}}</span> <span class=eav-entityselect-item-actions><span data-nodrag title=\"{{ 'FieldType.Entity.Edit' | translate }}\" ng-click=\"edit(item, index)\"><i icon=pencil></i></span> <span data-nodrag title=\"{{ 'FieldType.Entity.Remove' | translate }}\" ng-click=\"removeSlot(item, $index)\" class=eav-entityselect-item-remove><i icon=minus></i></span></span></div></li></ol></div><select class=\"eav-entityselect-selector form-control input-lg\" ng-model=selectedEntity ng-change=addEntity() ng-show=\"to.settings.merged.AllowMultiValue || chosenEntities.length < 1\"><option value=\"\" translate=FieldType.Entity.Choose></option><option value=new ng-if=createEntityAllowed() translate=FieldType.Entity.New></option><option ng-repeat=\"item in availableEntities\" ng-disabled=\"chosenEntities.indexOf(item.Value) != -1\" value={{item.Value}}>{{item.Text}}</option></select></div>"
  );


  $templateCache.put('form/edit-many-entities.html',
    "<div ng-if=\"vm.items != null\" ng-click=vm.debug.autoEnableAsNeeded($event)><eav-language-switcher is-disabled=!vm.isValid()></eav-language-switcher><div ng-repeat=\"p in vm.items\" class=group-entity><h3 class=clickable ng-click=\"p.collapse = !p.collapse\">{{p.Header.Title ? p.Header.Title : 'EditEntity.DefaultTitle' | translate }}&nbsp; <span ng-if=p.Header.Group.SlotCanBeEmpty ng-click=vm.toggleSlotIsEmpty(p) stop-event=click><switch ng-model=p.slotIsUsed class=tosic-blue style=\"top: 6px\" tooltip=\"{{'EditEntity.SlotUsed' + p.slotIsUsed | translate}}\"></switch></span> <span class=\"pull-right clickable\" style=\"font-size: smaller\"><span class=\"low-priority collapse-entity-button\" ng-if=p.collapse icon=plus-sign></span> <span class=collapse-entity-button ng-if=!p.collapse icon=minus-sign></span></span></h3><eav-edit-entity-form entity=p.Entity header=p.Header register-edit-control=vm.registerEditControl ng-hide=p.collapse></eav-edit-entity-form></div><div><button ng-disabled=\"!vm.isValid() || vm.isWorking\" ng-click=vm.save(true) type=button class=\"btn btn-primary btn-lg submit-button\"><span icon=ok tooltip=\"{{ 'Button.Save' | translate }}\"></span> &nbsp;<span translate=Button.Save></span></button> &nbsp; <button ng-disabled=\"!vm.isValid() || vm.isWorking\" class=\"btn btn-default btn-lg btn-square\" type=button ng-click=vm.save(false)><span icon=check tooltip=\"{{ 'Button.SaveAndKeepOpen' | translate }}\"></span></button> &nbsp;<switch ng-model=vm.willPublish class=tosic-blue style=\"top: 13px\"></switch>&nbsp; <span ng-click=\"vm.willPublish = !vm.willPublish;\" class=save-published-icon><i ng-if=vm.willPublish icon=eye-open tooltip=\"{{ 'Status.Published' | translate }} - {{ 'Message.WillPublish' | translate }}\"></i> <i ng-if=!vm.willPublish icon=eye-close tooltip=\"{{ 'Status.Unpublished' | translate }} - {{ 'Message.WontPublish' | translate }}\"></i></span> <span ng-if=vm.debug.on><button tooltip=debug icon=zoom-in class=btn ng-click=\"vm.showDebugItems = !vm.showDebugItems\"></button></span><show-debug-availability class=pull-right style=\"margin-top: 20px\"></show-debug-availability></div><div ng-if=\"vm.debug.on && vm.showDebugItems\"><pre>{{ vm.items | json }}</pre></div></div>"
  );


  $templateCache.put('form/edit-single-entity.html',
    "<div ng-show=vm.editInDefaultLanguageFirst() translate=Message.PleaseCreateDefLang></div><div ng-show=!vm.editInDefaultLanguageFirst()><formly-form ng-if=\"vm.formFields && vm.formFields.length\" ng-submit=vm.onSubmit() form=vm.form model=vm.entity.Attributes fields=vm.formFields></formly-form></div>"
  );


  $templateCache.put('form/main-form.html',
    "<div class=modal-body><span class=pull-right><span style=\"display: inline-block; position: relative; left:15px\"><button class=\"btn btn-default btn-square btn-subtle\" type=button ng-click=vm.close()><i icon=remove></i></button></span></span><eav-edit-entities item-list=vm.itemList after-save-event=vm.afterSave state=vm.state></eav-edit-entities></div>"
  );


  $templateCache.put('localization/formly-localization-wrapper.html',
    "<eav-localization-scope-control></eav-localization-scope-control><div ng-if=!!value><eav-localization-menu field-model=model[options.key] options=options value=value index=index></eav-localization-menu><formly-transclude></formly-transclude></div><p class=bg-info style=padding:12px ng-if=!value translate=LangWrapper.CreateValueInDefFirst translate-values=\"{ fieldname: '{{to.label}}' }\">Please... <i>'{{to.label}}'</i> in the def...</p>"
  );


  $templateCache.put('localization/language-switcher.html',
    "<tabset><tab ng-repeat=\"l in languages.languages\" heading=\"{{ l.name.substring(0, l.name.indexOf('(') > 0 ? l.name.indexOf('(') - 1 : 100 ) }}\" ng-click=\"!isDisabled ? languages.currentLanguage = l.key : false;\" disable=isDisabled active=\"languages.currentLanguage == l.key\" tooltip={{l.name}}></tab></tabset>"
  );


  $templateCache.put('localization/localization-menu.html',
    "<div dropdown is-open=status.isopen class=eav-localization style=\"z-index:{{1000 - index}}\"><a class=eav-localization-lock ng-click=vm.actions.toggleTranslate(); ng-if=vm.isDefaultLanguage() title={{vm.tooltip()}} ng-class=\"{ 'eav-localization-lock-open': !options.templateOptions.disabled }\" dropdown-toggle>{{vm.infoMessage()}} <i class=\"glyphicon glyphicon-globe\"></i></a><ul class=\"dropdown-menu multi-level pull-right eav-localization-dropdown\" role=menu aria-labelledby=single-button><li role=menuitem><a ng-disabled=vm.enableTranslate() ng-click=vm.actions.translate() translate=LangMenu.Unlink></a></li><li role=menuitem><a ng-click=vm.actions.linkDefault() translate=LangMenu.LinkDefault></a></li><li role=menuitem class=dropdown-submenu><a href=# translate=LangMenu.GoogleTranslate></a><ul class=dropdown-menu><li ng-repeat=\"language in vm.languages.languages\" class=disabled role=menuitem><a ng-click=vm.actions.autoTranslate(language.key) title={{language.name}} href=#>{{language.key}}</a></li></ul></li><li role=menuitem class=dropdown-submenu><a href=# translate=LangMenu.Copy></a><ul class=dropdown-menu><li ng-repeat=\"language in vm.languages.languages\" ng-class=\"{ disabled: options.templateOptions.disabled || !vm.hasLanguage(language.key) }\" role=menuitem><a ng-click=vm.actions.copyFrom(language.key) title={{language.name}} href=#>{{language.key}}</a></li></ul></li><li role=menuitem class=dropdown-submenu><a href=# translate=LangMenu.Use></a><ul class=dropdown-menu><li ng-repeat=\"language in vm.languages.languages\" ng-class=\"{ disabled: !vm.hasLanguage(language.key) }\" role=menuitem><a ng-click=vm.actions.useFrom(language.key) title={{language.name}} href=#>{{language.key}}</a></li></ul></li><li role=menuitem class=dropdown-submenu><a href=# translate=LangMenu.Share></a><ul class=dropdown-menu><li ng-repeat=\"language in vm.languages.languages\" ng-class=\"{ disabled: !vm.hasLanguage(language.key) }\" role=menuitem><a ng-click=vm.actions.shareFrom(language.key) title={{language.name}} href=#>{{language.key}}</a></li></ul></li></ul></div>"
  );


  $templateCache.put('wrappers/collapsible.html',
    "<div ng-show=!to.collapse class=group-field-set><formly-transclude></formly-transclude></div>"
  );


  $templateCache.put('wrappers/disablevisually.html',
    "<div visually-disabled={{to.disabled}}><formly-transclude></formly-transclude></div>"
  );


  $templateCache.put('wrappers/eav-label.html',
    "<div><label for={{id}} class=\"control-label {{to.labelSrOnly ? 'sr-only' : ''}} {{to.type}}\" ng-if=to.label>{{to.label}} {{to.required ? '*' : ''}} <a tabindex=-1 ng-click=\"to.showDescription = !to.showDescription\" href=javascript:void(0); ng-if=\"to.description && to.description != ''\"><i icon=info-sign class=low-priority></i></a></label><p ng-if=to.showDescription class=bg-info style=\"padding: 5px\" ng-bind-html=to.description></p><formly-transclude></formly-transclude></div>"
  );


  $templateCache.put('wrappers/field-group.html',
    "<div><h4 class=clickable ng-click=\"to.collapseGroup = !to.collapseGroup\">{{to.label}} <span class=\"pull-right btn-sm\"><span ng-if=to.collapseGroup class=\"low-priority collapse-fieldgroup-button\" icon=plus-sign></span> <span ng-if=!to.collapseGroup class=\"low-priority collapse-fieldgroup-button\" icon=minus-sign></span></span></h4><div ng-if=!to.collapseGroup style=\"padding: 5px\" ng-bind-html=to.description></div><formly-transclude></formly-transclude></div>"
  );

}]);

<%@ Control Language="C#" Inherits="ToSic.Eav.ManagementUI.Entity_Edit" CodeBehind="Entity_Edit.ascx.cs" AutoEventWireup="True" %>
<%@ Register Src="../Controls/DimensionMenu.ascx" TagPrefix="Eav" TagName="DimensionMenu" %>

<script type="text/javascript" src="/EAV/FieldTemplates/Entity_Edit.js"></script>
<link rel="stylesheet" href="/EAV/FieldTemplates/Entity_Edit.css"/>

<asp:Label ID="FieldLabel" runat="server" />

<div class="eav-field-control" ng-controller="EntityEditCtrl">
    
    <input style="display:none;" type="text" runat="server" id="hfConfiguration" ng-value="configuration | json" />
    <input style="display:none;" type="text" runat="server" id="hfEntityIds" ng-value="entityIds()" />

    <div ui-tree="options">
        <ol ui-tree-nodes ng-model="configuration.SelectedEntities">
            <li ng-repeat="item in configuration.SelectedEntities" ui-tree-node class="eav-entityselect-item">
                <div ui-tree-handle ng-init="itemText = (configuration.Entities | filter:{Value: item})[0].Text">
                    <span title="{{itemText + ' (' + item + ')'}}">{{itemText}}</span>
                    <a data-nodrag title="Remove this item" ng-click="remove(this)" class="eav-entityselect-item-remove">[remove]</a>
                </div>
            </li>
        </ol>
    </div>
        
    <select class="eav-entityselect-selector" ng-model="selectedEntity" ng-change="AddEntity()" ng-show="configuration.AllowMultiValue || configuration.SelectedEntities.length < 1">
        <option value="">{{configuration.AllowMultiValue ? '-- add more --' : '-- choose --'}}</option>
        <option ng-repeat="item in configuration.Entities" ng-disabled="configuration.SelectedEntities.indexOf(item.Value) != -1" value="{{item.Value}}">{{item.Text}}</option>
    </select>

</div>
<Eav:DimensionMenu ID="DimensionMenu1" runat="server" />
<br />

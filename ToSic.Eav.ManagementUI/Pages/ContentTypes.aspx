<%@ Page Title="Content Types" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" Inherits="ToSic.Eav.ManagementUI.Pages.ContentTypes" Codebehind="ContentTypes.aspx.cs" %>

<%@ Register src="~/EAV/Controls/ContentTypesList.ascx" tagname="ContentTypesList" tagprefix="Eav" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
	<Eav:ContentTypesList ID="ContentTypesList1" runat="server" UseDialogs="True" AppId="1" />
</asp:Content>
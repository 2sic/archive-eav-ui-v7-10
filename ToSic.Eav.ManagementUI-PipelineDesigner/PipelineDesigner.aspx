<%@ Page Title="Pipeline Designer" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="PipelineDesigner.aspx.cs" Inherits="ToSic.Eav.ManagementUI.PipelineDesigner" %>
<%@ Register src="EAV/PipelineDesigner/PipelineDesigner.ascx" tagname="PipelineDesigner" tagprefix="Eav" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
	<script type="text/javascript" src="Scripts/jquery-1.9.1.min.js"></script>
	<script type="text/javascript" src="Scripts/jquery-ui-1.10.2.min.js"></script>
	<script type="text/javascript" src="Scripts/jquery.jsPlumb-1.4.1-all-min.js"></script>
	<script type="text/javascript" src="Scripts/pipelineDesigner.js"></script>
	<link rel="stylesheet" href="EAV/PipelineDesigner/demo-new.css" />
	<link rel="stylesheet" href="EAV/PipelineDesigner/flowchartDemo.css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<Eav:PipelineDesigner ID="PipelineDesigner1" runat="server" />
</asp:Content>

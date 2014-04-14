<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="ToSic.Eav.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
	    <div><strong>Microtests</strong><ul>
		    <li><a href="?Test=All">All</a></li>
		    <li><a href="?Test=TypeFilter">Type-Filter</a></li>
			<li><a href="?Test=ValueFilter">ValueFilter</a></li>
			<li><a href="?Test=AttributeFilter">Attribute Stripper/Filter</a></li>
			<li><a href="?Test=ValueSort">ValueSort</a></li>
			<li><a href="?Test=listproperty">Quick Property to access the Out["Default"].List</a></li>
	    </ul></div>
		
		<asp:Literal runat="server" ID="litResults" />
    </form>
</body>
</html>

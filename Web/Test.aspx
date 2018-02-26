<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="Web.Test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>    
</head>
<body>
    <ul id="ul">
        <li>One</li>
        <li>Two</li>
    </ul>
    <input type="button" value="Create Service" id="CreateService" />
    <div id="Services"></div>
    <script type="text/javascript" src="Scripts/JQuery_1.12.0.js"></script>    
    <script type="text/javascript" src="Scripts/OrdersClient.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            var ordersClient = new OrdersClient({ async: true });
            ordersClient.DeleteQuotation(1, false, function (res) {

            });
        });
    </script>
</body>
</html>

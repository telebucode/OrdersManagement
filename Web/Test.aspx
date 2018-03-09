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
            //ordersClient.GetBankAccounts(true, function (res) {
            //    console.log(res);
            //});
            //ordersClient.GetPaymentGateways(true, function (res) {
            //    console.log(res);
            //});
            //ordersClient.ViewQuotation(4012, false, function (res) {
            //    console.log(res);
            //    $("#Services").html(res);
            //});
            //ordersClient.DownloadQuotation(4012, false, function (res) {
            //    console.log(res);
            //    var a = document.createElement('a');
            //    a.href = "http://localhost:4018/" + res.FilePath;
            //    a.download = "http://localhost:4018/" + res.FilePath;
            //    document.body.appendChild(a);
            //    a.click();
            //});
            //var invoiceSearchData = {};
            //invoiceSearchData.AccountId = 0;
            //invoiceSearchData.ProductId = 1;
            //invoiceSearchData.InvoiceId = 0;
            //invoiceSearchData.QuotationNumber = "";
            //invoiceSearchData.EmployeeId = 0;
            //invoiceSearchData.OwnerShipId = 0;
            //invoiceSearchData.ChannelId = 2;
            //invoiceSearchData.BillingModeId = 0;
            //invoiceSearchData.PageNumber = 1;
            //invoiceSearchData.Mobile = "";
            //invoiceSearchData.Email = "";
            //invoiceSearchData.Limit = 20;
            //invoiceSearchData.FromDateTime = "2018-02-01";
            //ordersClient.GetInvoices(invoiceSearchData, function (res) {
            //    console.log(res);
            //    $("#Services").html(res);
            //});
            var searchData = {};
            searchData.ProductId = 1;
            searchData.AccountId = 1;
            searchData.BillingMode = 1;
            searchData.FromDateTime = '2018-01-01';
            searchData.ToDateTime = '2018-03-30';
            //getOrders(searchData);
            //function getOrders(ordersSearchData) {
            //    ordersClient.GetOrders(ordersSearchData, function (res) {
            //        console.log(res);
            //    });
            //}
            getPayments(searchData);
            function getPayments(ordersSearchData) {
                ordersClient.GetPayments(ordersSearchData, function (res) {
                    console.log(res);
                });
            }
        });
    </script>
</body>
</html>

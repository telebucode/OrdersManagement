var servicesHandler = "AjaxHandlers/Services.ashx";
var quotationsHandler = "AjaxHandlers/Quotations.ashx";
var servicesMetaData = "";
$(document).ready(function () {

})
function GetServices(serviceId, onlyActive, includeServiceProperties)
{
    $.ajax({
        url: servicesHandler,
        data: { Action: "GetServices", ServiceId: serviceId, OnlyActive: onlyActive, IncludeServiceProperties: includeServiceProperties },
        dataType: "JSON",
        success: function (response) {
            if (response.Success == true)
                servicesMetaData = response.Services;
        },
        error: function (response) {
            var obj = { Success: false, Message: "Handler Returned Non-Success Response Code" };
        }
    })
}
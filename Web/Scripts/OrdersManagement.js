var servicesHandler = "AjaxHandlers/Services.ashx";
var quotationsHandler = "AjaxHandlers/Quotations.ashx";
var servicesMetaData = [];
var inputTypes = {};
var inputDataTypes = {}
$(document).ready(function () {

});
function GetInputTypes(onlyActive)
{
    $.ajax({
        url: servicesHandler,
        data: { Action: "GetInputTypes", OnlyActive: onlyActive },
        dataType: "JSON",
        success:function(response)
        {
            if (response.Success == true)
                inputTypes = response.inputTypes;
            return inputTypes;
        },
        error: function (response)
        {
            var obj = { Success: false, Message: "Handler Returned Non-Success Response Code" };
            return obj;
        }
    })
}
function GetServices(serviceId, onlyActive, includeServiceProperties)
{
    $.ajax({
        url: servicesHandler,
        async: false,
        data: { Action: "GetServices", ServiceId: serviceId, OnlyActive: onlyActive, IncludeServiceProperties: includeServiceProperties },
        dataType: "JSON",
        success: function (response) {
            //if (response.Success == true) {
            //    servicesMetaData = response;
            //    console.log(servicesMetaData);
            //}
            servicesMetaData = response;
        },
        error: function (response) {
            servicesMetaData = { Success: false, Message: "Handler Returned Non-Success Response Code", Response: response };            
        }
    });
    return servicesMetaData;
}
//$.getScript('Scripts/OrdersManagement_Plugin.js', function () {
    (function () {
        var servicesMetaData;
        var defaultErrorMessage = "Handler Returned Non-Success Response Code";
        var failedActionResponse = { Success: false, Message: defaultErrorMessage }
        var isAsync = true;
        // Constructor Start
        this.OrdersClient = function (options) {
            // Define option defaults
            var defaults = {
                servicesHandler: "/AjaxHandlers/Service.ashx",
                quotationsHandler: "/AjaxHandlers/Quotation.ashx",
                invoicesHandler: "/AjaxHandlers/Invoice.ashx",
                paymentsHandler: "/AjaxHandlers/Payment.ashx",
                activationsHandler: "/AjaxHandlers/Activation.ashx",
                genericHandler: "/AjaxHandlers/Generic.ashx",
                async: true
            }
            // Create options by extending defaults with the passed in arugments
            if (arguments[0] && typeof arguments[0] === "object")
                this.options = extendDefaults(defaults, arguments[0]);
            else
                this.options = defaults;
            isAsync = this.options.async;
            InitializeEvents();
        }
        // Constructor End

        // Private Methods Start

        // Utility method to extend defaults with user options
        function extendDefaults(source, properties) {
            var property;
            for (property in properties) {
                if (properties.hasOwnProperty(property)) {
                    source[property] = properties[property];
                }
            }
            return source;
        }
        function InitializeEvents() {
            var createServiceButton = document.getElementById("CreateService");
            if (createServiceButton != null)
                createServiceButton.addEventListener("click", function () { alert("hai"); });
        }
        function CanCallBack(callBackFunction)
        {   
            if (isAsync && callBackFunction && typeof callBackFunction === "function")
                return true;
            else
                return false;
        }
        function RenderServices(actionResponse)
        {
            html = '<table class="table table-responsive" border="1"><thead><tr>';
            Object.keys(actionResponse.Services[0]).forEach(function (key) {
                if (key != "Properties")
                    html += '<th>' + key + '</th>';
            });
            html += '</tr></thead><tbody>';
            for (var i = 0; i < actionResponse.Services.length; i++) {
                html += "<tr>"
                Object.keys(actionResponse.Services[i]).forEach(function (key) {
                    if (key != "Properties")
                        html += "<td>" + actionResponse.Services[i][key] + "</td>";
                });
            }
            html += "</tbody></table>";
            document.getElementById("Services").innerHTML = html;
        }
        // Private Methods End

        // Public Methods Start
        // Services Related
        OrdersClient.prototype.GetInputTypes = function (onlyActive, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.servicesHandler,
                async: this.options.async,
                dataType: "JSON",
                data:
                    {
                        Action: "GetInputTypes",
                        OnlyActive: onlyActive ? onlyActive : true
                    },
                success: function (response) {
                    actionResponse = response;
                    if(CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if(!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.GetInputDataTypes = function (onlyActive, callBackFunction) {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.servicesHandler,
                async: this.options.async,
                dataType: "JSON",
                data:
                    {
                        Action: "GetInputDataTypes",
                        OnlyActive: onlyActive ? onlyActive : true
                    },
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.GetServices = function (productId,serviceId, onlyActive, includeServiceProperties, renderAutomatically, callBackFunction) {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.servicesHandler,
                async: this.options.async,
                data: {
                    Action: "GetServices",
                    ServiceId: (serviceId && serviceId > 0) ? serviceId : 0,
                    OnlyActive: onlyActive ? onlyActive : true,
                    ProductId : productId,
                    IncludeServiceProperties: includeServiceProperties ? includeServiceProperties : true
                },
                dataType: "JSON",
                success: function (response) {                    
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                    if (renderAutomatically && document.getElementById("Services") != null && actionResponse.Success && actionResponse.Services.length > 0)
                        RenderServices(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }                
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.CreateService = function (displayName, metaDataCode, areMultiplesEntriesAllowed, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.servicesHandler,
                async: this.options.async,
                method: "POST",
                data: {
                    Action: "CreateService",
                    DisplayName: displayName,
                    MetaDataCode: metaDataCode,
                    AreMultipleEntriesAllowed: areMultiplesEntriesAllowed
                },
                dataType: "JSON",
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.UpdateService = function (serviceId, displayName, metaDataCode, areMultipleEntriesAllowed, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.servicesHandler,
                async: this.options.async,
                method: "POST",
                data:
                    {
                        Action: "UpdateService",
                        ServiceId: serviceId,
                        DisplayName: displayName,
                        MetaDataCode: metaDataCode,
                        AreMultipleEntriesAllowed: areMultipleEntriesAllowed
                    },
                dataType: "JSON",
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.DeleteService = function(serviceId, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.servicesHandler,
                async: this.options.async,
                method: "POST",
                data:
                    {
                        Action: "DeleteService",
                        ServiceId: serviceId
                    },
                dataType: "JSON",
                success:function(response)
                {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error:function(response)
                {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.CreateServiceProperties = function (serviceId, properties, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.servicesHandler,
                async: this.options.async,
                method: "POST",
                traditional: true,
                data:
                    {
                        Action: "CreateServiceProperties",
                        ServiceId: serviceId,
                        Properties: JSON.stringify(properties)
                    },
                dataType: "JSON",
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        // Quotations Related
        OrdersClient.prototype.GetQuotationStatuses = function(onlyActive, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "JSON",
                data:
                    {
                        Action: "GetQuotationStatuses",
                        OnlyActive: onlyActive ? onlyActive : true
                    },
                success:function(response)
                {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error:function(response)
                {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.GetQuotationChannels = function(onlyActive, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "JSON",
                data:
                    {
                        Action: "GetQuotationChannels",
                        OnlyActive: onlyActive ? onlyActive : true
                    },
                success:function(response)
                {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error:function(response)
                {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.GetQuotations = function(searchData, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "JSON",
                traditional: true,
                data:
                    {
                        Action: "Search",
                        SearchData: JSON.stringify(searchData)
                    },
                success:function(response)
                {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error:function(response)
                {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.GetQuotationDetails = function (quotationId, isPostPaidQuotation, callBackFunction) {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "JSON",
                traditional: true,
                data:
                    {
                        "Action": "GetQuotationDetails",
                        "QuotationId": quotationId ? quotationId : 0,
                        "IsPostPaidQuotation": isPostPaidQuotation ? isPostPaidQuotation : false

                    },
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.ViewQuotation = function (quotationId, isPostPaidQuotation, callBackFunction) {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "text",
                traditional: true,
                data:
                    {
                        "Action": "View",
                        "QuotationId": quotationId ? quotationId : 0,
                        "IsPostPaidQuotation": isPostPaidQuotation ? isPostPaidQuotation : false

                    },
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.DownloadQuotation = function (quotationId, isPostPaidQuotation, callBackFunction) {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "text",
                traditional: true,
                data:
                    {
                        "Action": "Download",
                        "QuotationId": quotationId ? quotationId : 0,
                        "IsPostPaidQuotation": isPostPaidQuotation ? isPostPaidQuotation : false
                    },
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.CreateQuotation = function(accountId, employeeId, channelId, metaData, stateId, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "JSON",
                traditional: true,
                data:
                    {
                        Action: "Create",
                        AccountId: accountId ? accountId : 0,
                        EmployeeId: employeeId ? employeeId : 0,
                        ChannelId: channelId,
                        MetaData: JSON.stringify(metaData)
                    },
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.UpdateQuotation = function (quotationId, employeeId, channelId, metaData, stateId, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "JSON",
                traditional: true,
                data:
                    {
                        Action: "Update",
                        QuotationId: quotationId ? quotationId : 0,
                        EmployeeId: employeeId ? employeeId : 0,
                        channelId: channelId ? channelId : 0,
                        MetaData: JSON.stringify(metaData)
                    },
                success: function (response)
                {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response)
                {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.DeleteQuotation = function (quotationId, isPostPaidQuotation, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.quotationsHandler,
                async: this.options.async,
                dataType: "JSON",
                data:
                    {
                        "Action": "Delete",
                        "QuotationId": quotationId ? quotationId : 0,
                        "IsPostPaidQuotation": isPostPaidQuotation
                    },
                success: function (response)
                {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response)
                {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        // Invoices Related
        OrdersClient.prototype.GetInvoiceStatuses = function(onlyActive, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.invoicesHandler,
                async: this.options.async,
                dataType: "JSON",
                data:
                    {
                        "Action": "GetStatuses",
                        "OnlyActive": onlyActive ? onlyActive : true
                    },
                success:function(response)
                {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error:function(response)
                    {
                        failedActionResponse.Response = response;
                        failedActionResponse.Message = response.responseJSON.Message;
                        actionResponse = failedActionResponse;
                        if (CanCallBack(callBackFunction))
                            callBackFunction(actionResponse);
                    }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.CreateInvoice = function(quotationId, billingModeId, employeeId, callBackFunction)
        {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.invoicesHandler,
                async: this.options.async,
                dataType: "JSON",
                data:
                    {
                        Action: "Create",
                        QuotationId: quotationId ? quotationId : 0,
                        BillingModeId: billingModeId ? billingModeId : 0,
                        EmployeeId: employeeId ? employeeId : 0
                    },
                success:function(response)
                {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error:function(response)
                {
                    failedActionResponse.Response = actionResponse;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.ViewInvoice = function (quotationId, isPostPaidQuotation, callBackFunction) {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.invoicesHandler,
                async: this.options.async,
                dataType: "text",
                traditional: true,
                data:
                    {
                        "Action": "View",
                        "QuotationId": quotationId ? quotationId : 0,
                        "IsPostPaidQuotation": isPostPaidQuotation ? isPostPaidQuotation : false

                    },
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        OrdersClient.prototype.DownloadInvoice = function (quotationId, isPostPaidQuotation, callBackFunction) {
            var actionResponse;
            failedActionResponse.Message = defaultErrorMessage;
            $.ajax({
                url: this.options.invoicesHandler,
                async: this.options.async,
                dataType: "text",
                traditional: true,
                data:
                    {
                        "Action": "Download",
                        "QuotationId": quotationId ? quotationId : 0,
                        "IsPostPaidQuotation": isPostPaidQuotation ? isPostPaidQuotation : false

                    },
                success: function (response) {
                    actionResponse = response;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                },
                error: function (response) {
                    failedActionResponse.Response = response;
                    failedActionResponse.Message = response.responseJSON.Message;
                    actionResponse = failedActionResponse;
                    if (CanCallBack(callBackFunction))
                        callBackFunction(actionResponse);
                }
            });
            if (!CanCallBack(callBackFunction))
                return actionResponse;
        }
        // Public Methods End
    }());
//});
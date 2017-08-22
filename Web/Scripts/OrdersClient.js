(function () {
    // Constructor Start
    this.OrdersClient = function (options) {
        // Define option defaults
        var defaults = {
            servicesHandler: "AjaxHandlers/Services.ashx",
            quotationsHandler: "AjaxHandlers/Quotations.ashx",
            invoicesHandler: "AjaxHandlers/Invoices.ashx",
            paymentsHandler: "AjaxHandlers/Payments.ashx",
            activationsHandler: "AjaxHandlers/Activations.ashx",
            genericHandler: "AjaxHandlers/Generic.ashx"
        }
        // Create options by extending defaults with the passed in arugments
        if (arguments[0] && typeof arguments[0] === "object")
            this.options = extendDefaults(defaults, arguments[0]);
        else
            this.options = defaults;

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
    function InitializeEvents() { }
    // Private Methods End

    // Public Methods Start
    OrdersClient.prototype.GetServices = function (serviceId, onlyActive, includeServiceProperties) {
        var servicesMetaData;
        $.ajax({
            url: this.options.servicesHandler,
            async: false,
            data: {
                Action: "GetServices",
                ServiceId: (serviceId && serviceId > 0) ? serviceId : 0,
                OnlyActive: onlyActive ? onlyActive : true,
                IncludeServiceProperties: includeServiceProperties ? includeServiceProperties : true
            },
            dataType: "JSON",
            success: function (response) {
                servicesMetaData = response;
            },
            error: function (response) {
                servicesMetaData = { Success: false, Message: "Handler Returned Non-Success Response Code", Response: response };
            }
        });
        return servicesMetaData;
    }
    // Public Methods End
}());
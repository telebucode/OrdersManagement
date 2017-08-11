(function ($) {
    $.fn.pulse = function (options) {
        // Merge passed options with defaults
        var opts = jQuery.extend({}, jQuery.fn.pulse.defaults, options);
        return this.each(function () {
            obj = $(this);
            for (var i = 0; i < opts.pulses; i++) {
                obj.fadeTo(opts.speed, opts.fadeLow).fadeTo(opts.speed, opts.fadeHigh);
            };
            // Reset to normal
            obj.fadeTo(opts.speed, 1);
        });
    };
    $.fn.increase = function () {
        return this.each(function () {
            obj = $(this);
            obj.css("font-size", "50px");
        });
    };
    $.fn.decrease = function () {
        return this.each(function () {
            obj = $(this);
            obj.css("font-size", "20px");
        })
    };
    $.fn.PopulateServices = function (options) {

    };
    // Pulse plugin default options
    jQuery.fn.pulse.defaults = {
        speed: "slow",
        pulses: 2,
        fadeLow: 0.2,
        fadeHigh: 1
    };
})(jQuery);
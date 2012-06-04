/// <reference path="~/Scripts/knockout-2.1.0.debug.js" />
/// <reference path="~/Scripts/raphael.js" />
/// <reference path="~/Scripts/g.raphael.js" />
/// <reference path="~/Scripts/g.bar.js" />
/// <reference path="~/Scripts/g.dot.js" />
/// <reference path="~/Scripts/g.line.js" />
/// <reference path="~/Scripts/g.pie.js" />

ko.bindingHandlers.graph = (function () {
    var createGraph = function (r, value) {
        var data = ko.utils.unwrapObservable(value.data);
        var options = ko.utils.unwrapObservable(value.options);
        switch (value.type) {
            case 'pie':
                return r.piechart(value.x || value.width / 2, value.y || value.height / 2, value.radius || (value.height / 2), data, options);
            case 'bar':
                return r.barchart(0, 0, value.width, value.height, data, options);
        }
    };

    return {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            var r = Raphael(element, value.width || 100, value.height || 100);
            element.Raphael = r;
        },
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            var r = element.Raphael;
            var c = element.Chart;
            if (c) {
                c.remove();
            }

            element.Chart = createGraph(r, value);
        }
    };
})();
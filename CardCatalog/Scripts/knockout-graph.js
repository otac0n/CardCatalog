/// <reference path="~/Scripts/knockout-2.1.0.debug.js" />
/// <reference path="~/Scripts/raphael.js" />
/// <reference path="~/Scripts/g.raphael.js" />

ko.bindingHandlers.graph = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = ko.utils.unwrapObservable(valueAccessor());
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = ko.utils.unwrapObservable(valueAccessor());
    }
};
/// <reference path="~/Scripts/jquery-1.7.2-vsdoc.js" />
/// <reference path="~/Scripts/knockout-2.1.0.debug.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />

var search = function () { };

var deck = (function () {
    var vm = ko.mapping.fromJS(initialDeck);

    while (vm.Columns().length < 5) {
        vm.Columns.push(ko.mapping.fromJS({ Cards: [] }));
    }

    vm.addCard = function (card) {
        var cols = vm.Columns();

        if (cols.length == 0) {
            vm.Columns.push(ko.mapping.fromJS({ Cards: [] }));
            cols = vm.Columns();
        }

        cols[cols.length - 1].Cards.push(ko.mapping.fromJS(card));
    };

    ko.applyBindings(vm, $("#deck")[0]);
    return vm;
})();

var searchResults = (function () {
    var vm = ko.mapping.fromJS({ CardsInfo: [], Page: 0, Pages: 0 });

    vm.addCard = function (card) {
        deck.addCard(ko.mapping.toJS(card));
    };

    vm.pageChange = function (delta) {
        search(vm.Page() + delta);
    };

    ko.applyBindings(vm, $("#search-results")[0]);
    return vm;
})();

$(function () {
    var visualSearch = VS.init({
        container: $('#card-search'),
        query: '',
        callbacks: {
            search: function (query, searchCollection) { search(1); },
            facetMatches: function (callback) { callback(['artist', 'cost', 'color', 'expansion', 'name', 'owned', 'power', 'rarity', 'toughness', 'type']); },
            valueMatches: function (facet, searchTerm, callback) {
                switch (facet.toLowerCase()) {
                    case 'color':
                        callback(['black', 'blue', 'green', 'red', 'white'], { preserveMatches: true });
                        break;
                    case 'expansion':
                        callback(expansions, { preserveMatches: true });
                        break;
                    case 'owned':
                        callback(['true', 'false'], { preserveMatches: true });
                        break;
                    case 'rarity':
                        callback(['basic', 'common', 'uncommon', 'rare', 'mythic'], { preserveMatches: true });
                        break;
                }
            }
        }
    });

    search = function (page) {
        searchResults.CardsInfo.remove(function () { return true; });

        var facets = visualSearch.searchQuery.facets();
        var terms = page > 1 ? ("?page=" + page) : "";
        for (var i = 0; i < facets.length; i++) {
            for (var k in facets[i]) {
                terms += (terms == '' ? "?" : "&") + encodeURIComponent(k) + "=" + encodeURIComponent(facets[i][k]);
            }
        }

        $.ajax({
            url: Urls.cardSearch + terms,
            dataType: 'json',
            success: function (data) {
                searchResults.Page(data.Page);
                searchResults.Pages(data.Pages);

                for (var i = 0; i < data.CardsInfo.length; i++) {
                    searchResults.CardsInfo.push(data.CardsInfo[i]);
                }
            },
            error: function (_, status) {
                // TODO: Switch to a message in the DOM once we have a target for search results.
                alert("There was an error communicating with the server.");
            }
        });
    };

    visualSearch.searchBox.value("owned: true");
    search(1);
});

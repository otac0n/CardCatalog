/// <reference path="~/Scripts/jquery-1.7.2-vsdoc.js" />
/// <reference path="~/Scripts/knockout-2.1.0.debug.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />

var deck = (function () {
    var vm = ko.mapping.fromJS(initialDeck);

    vm.removeCard = function (card) {
        vm.cards.remove(card);
    };

    ko.applyBindings(vm, $("#deck")[0]);
    return vm;
})();

var searchResults = (function () {
    var vm = ko.mapping.fromJS({ cards: [] });

    vm.addCard = function (card) {
        deck.cards.push(ko.mapping.fromJS(ko.mapping.toJS(card)));
    };

    ko.applyBindings(vm, $("#search-results")[0]);
    return vm;
})();

var search = (function () {
    return function search(facets) {
        searchResults.cards.remove(function () { return true; });

        var terms = "";
        for (var i = 0; i < facets.length; i++) {
            for (var k in facets[i]) {
                terms += (terms == '' ? "?" : "&") + encodeURIComponent(k) + "=" + encodeURIComponent(facets[i][k]);
            }
        }

        $.ajax({
            url: Urls.cardSearch + terms,
            dataType: 'json',
            success: function (data) {
                for (var i = 0; i < data.length; i++) {
                    searchResults.cards.push(ko.mapping.fromJS(data[i]));
                }
            },
            error: function (_, status) {
                // TODO: Switch to a message in the DOM once we have a target for search results.
                alert("There was an error communicating with the server.");
            }
        });
    }
})();

$(function () {
    var visualSearch = VS.init({
        container: $('#card-search'),
        query: '',
        callbacks: {
            search: function (query, searchCollection) { search(searchCollection.facets()); },
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

    visualSearch.searchBox.value("owned: true");
    search([{ owned: "true"}]);
});

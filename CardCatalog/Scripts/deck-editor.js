/// <reference path="~/Scripts/jquery-1.7.2-vsdoc.js" />
/// <reference path="~/Scripts/knockout-2.1.0.debug.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />

var search = function () { };

var deck = (function () {
    var vm = ko.mapping.fromJS(initialDeck);
    vm.ActiveCard = ko.observable(null);

    vm.ensureEmptyColumn = function () {
        var cols = vm.Columns();

        for (var i = cols.length - 1; i > 0; i--) {
            if (cols[i].Cards().length == 0 &&
                cols[i - 1].Cards().length == 0) {
                vm.Columns.splice(i, 1);
                cols = vm.Columns();
            }
        }

        if (cols.length == 0 || cols[cols.length - 1].Cards().length != 0) {
            vm.Columns.push(ko.mapping.fromJS({ Cards: [] }));
        }
    };

    vm.addCard = function (card) {
        vm.Columns()[0].Cards.push(ko.mapping.fromJS(card));
    };

    var primaryTypes = [
        { match: /creature|summon|eaturecray/i, type: "Creature" },
        { match: /instant|interrupt/i, type: "Instant" },
        { match: /artifact/i, type: "Artifact" },
        { match: /sorcery/i, type: "Sorcery" },
        { match: /land/i, type: "Land" },
        { match: /enchant(ment)?/i, type: "Enchantment" },
        { match: /planeswalker/i, type: "Planeswalker" },
    ];

    vm.Stats = {
        CardCount: ko.computed(function () {
            var count = 0;

            var cols = this.Columns();
            for (var i = 0; i < cols.length; i++) {
                count += cols[i].Cards().length;
            }

            return count;
        }, vm),
        CardTypes: ko.computed(function () {
            var types = {};

            var cols = this.Columns();
            for (var i = 0; i < cols.length; i++) {
                var cards = cols[i].Cards();
                for (var c = 0; c < cards.length; c++) {
                    var t = cards[c].NormalizedFaces()[0].Types();

                    for (var p = 0; p < primaryTypes.length; p++) {
                        var pt = primaryTypes[p];
                        if (pt.match.test(t)) {
                            t = pt.type;
                            break;
                        }
                    }

                    types[t] = (types[t] || 0) + 1;
                }
            }

            var results = [];
            for (var t in types) {
                results.push({ Type: t, Count: types[t] });
            }

            results.sort(function (a, b) {
                if (a.Count > b.Count) {
                    return -1;
                } else if (a.Count < b.Count) {
                    return +1;
                } else if (a.Type < b.Type) {
                    return -1;
                } else if (a.Type < b.Type) {
                    return +1;
                } else {
                    return 0;
                }
            });

            return results;
        }, vm),
        ManaCosts: ko.computed(function () {
            var costs = [0, 0, 0, 0, 0, 0, 0, 0, 0];

            var cols = this.Columns();
            for (var i = 0; i < cols.length; i++) {
                var cards = cols[i].Cards();
                for (var c = 0; c < cards.length; c++) {
                    var m = cards[c].NormalizedFaces()[0].ConvertedManaCost();
                    costs[Math.min(m, costs.length - 1)]++;
                }
            }

            return costs;
        }, vm)
    };

    ko.applyBindings(vm, $("#deck")[0]);
    ko.applyBindings(vm, $("#stats-tab")[0]);
    vm.ensureEmptyColumn();
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

    $("#tabs").tabs();
    if (deck.Stats.CardCount() > 0) {
        $("#tabs").tabs("select", "#organize-tab");
    }

    visualSearch.searchBox.value("owned: true");
    search(1);
});

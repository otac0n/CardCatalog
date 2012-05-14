﻿var search = (function () {
    return function search(facets) {
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
                var $results = $("#search-results");
                $results.empty();
                for (var i = 0; i < data.length; i++) {
                    $results.append($("<img />").attr('src', Urls.imageCard.replace("__id__", data[i].Id).replace("__side__", 0)));
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
            facetMatches: function (callback) { callback([ 'artist', 'cost', 'color', 'expansion', 'name', 'owned', 'power', 'rarity', 'toughness', 'type' ]); },
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
});
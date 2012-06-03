/// <reference path="~/Scripts/jquery-1.7.2-vsdoc.js" />

$(function () {
    $("#search-box").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: Urls.cardFind,
                dataType: "json",
                data: { q: request.term },
                success: function (data) {
                    response($.map(data.Results, function (item) {
                        return {
                            label: item.Name,
                            desc: item.Snippet,
                            id: item.ID,
                            value: item.Name
                        };
                    }));
                }
            });
        },
        minLength: 2,
        select: function (event, ui) {
            if (ui.item) {
                window.location = Urls.cardDetails.replace('__id__', ui.item.id);
            }
        }
    })
    .data("autocomplete")._renderItem = function (ul, item) {
        return $("<li></li>")
            .data("item.autocomplete", item)
            .append('<a>' + item.label + '<br><span style="font-size: 8pt;">' + item.desc + '</span></a>')
            .appendTo(ul);
    };
    $(document).bind('keyup.s', function () {
        $("#search-box").focus().select(); return false;
    });
});
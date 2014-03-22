/// <reference path="../jquery.js" />
/*
jquery-makeTableScrollable.js
Version 0.1 - 5/30/2011
Make Table Scrollable plug-in

(c) 2011 Rick Strahl, West Wind Technologies 
www.west-wind.com

Licensed under MIT License
http://en.wikipedia.org/wiki/MIT_License

Simple plugin that makes an ordinary table scrollable
by scrolling the tbody section underneath the header.
*/
(function ($) {

$.fn.makeTableScrollable = function (options) {
    return this.each(function () {
        var $table = $(this);

        var opt = {
            // height of the table
            height: "250px",
            // right padding added to support the scrollbar
            rightPadding: "10px",
            // cssclass used for the wrapper div
            cssClass: "blackborder"
        }
        $.extend(opt, options);

        var $thead = $table.find("thead");
        var $ths = $thead.find("th");
        var id = $table.attr("id");
        var cssClass = $table.attr("class");

        if (!id)
            id = "_table_" + new Date().getMilliseconds().ToString();

        $table.width("+=" + opt.rightPadding);
        $table.css("border-width", 0);

        // add a column to all rows of the table
        var first = true;
        $table.find("tr").each(function () {
            var row = $(this);
            if (first) {
                row.append($("<th>").width(opt.rightPadding));
                first = false;
            }
            else
                row.append($("<td>").width(opt.rightPadding));
        });

        // force full sizing on each of the th elemnts
        $ths.each(function () {
            var $th = $(this);
            $th.css("width", $th.width());
        });

        // Create the table wrapper div
        var $tblDiv = $("<div>").css({ position: "relative",
            overflow: "hidden",
            overflowY: "scroll"
        })
                                    .addClass(opt.cssClass);
        var width = $table.width();
        $tblDiv.width(width).height(opt.height)
                .attr("id", id + "_wrapper")
                .css("border-top", "none");
        // Insert before $tblDiv
        $tblDiv.insertBefore($table);
        // then move the table into it
        $table.appendTo($tblDiv);

        // Clone the div for header
        var $hdDiv = $tblDiv.clone();
        $hdDiv.empty();
        var width = $table.width();
        $hdDiv.attr("style", "")
                .css("border-bottom", "none")
                .width(width)
                .attr("id", id + "_wrapper_header");

        // create a copy of the table and remove all children
        var $newTable = $($table).clone();
        $newTable.empty()
                    .attr("id", $table.attr("id") + "_header");

        $thead.appendTo($newTable);
        $hdDiv.insertBefore($tblDiv);
        $newTable.appendTo($hdDiv);

        $table.css("border-width", 0);
    });
}
})(jQuery);
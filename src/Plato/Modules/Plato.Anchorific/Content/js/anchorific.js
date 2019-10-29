﻿// <reference path="~/js/app/plato.js" />
// <reference path="~/js/vendors/jquery.js" />
// <reference path="~/js/vendors/bootstrap.js" />

if (typeof window.jQuery === "undefined") {
    throw new Error("jQuery 3.3.1 or above Required");
}

if (typeof window.$().modal === 'undefined') {
    throw new Error("BootStrap 4.1.1 or above Required");
}

if (typeof window.$.Plato === "undefined") {
    throw new Error("$.Plato Required");
}

(function ($, win, doc, undefined) {

    "use strict";

    var anchorific = function () {

        var dataKey = "anchorific",
            dataIdKey = dataKey + "Id";

        var defaults = {
                title: "Table of Contents",
                navigation: '.anchorific', // position of navigation
                headers: 'h1, h2, h3, h4, h5, h6', // custom headers selector                
                anchorClass: 'anchor text-muted', // class of anchor links                
                anchorTitle: null,
                iconCss: "fal fa-link",
                spy: true, // scroll spy
                position: 'append', // position of anchor icon
                spyOffset: !0, // specify heading offset for spy scrolling
                onAnchorClick: null, // triggers when an acnhor link is clicked
                onNavClick: null
            },
            methods = {
                alreadyAdded: [],
                init: function ($caller, methodName) {

                    if (methodName) {
                        if (this[methodName] !== null && typeof this[methodName] !== "undefined") {
                            this[methodName].apply(this, [$caller]);
                        } else {
                            alert(methodName + " is not a valid method!");
                        }
                        return null;
                    }

                    this.bind($caller);

                },
                bind: function ($caller) {

                    this.headers = $caller.find($caller.data(dataKey).headers);
                    this.previous = 0;

                    if (this.headers.length !== 0) {
                        this.first = parseInt(this.headers.prop('nodeName').substring(1), null);
                    }

                    this.build($caller);

                },
                build: function ($caller) {

                    var self = this,
                        obj,
                        navigations = function () { };

                    // when navigation configuration is set
                    var navSelector = $caller.data(dataKey).navigation;
                    if (navSelector) {
                 
                        var $h = $("<h3 />")
                            .append($("<i />", {
                                class: "fal fa-align-left mr-2 small"
                            }))
                            .append($("<span/>").text($caller.data(dataKey).title));

                        // Add table of contents generated from headers
                        $caller.prepend($('<nav class="anchorific"></nav>'));                    
                        $(navSelector).append($h);
                        $(navSelector).append('<hr />');
                        $(navSelector).append('<ul />');                        
                        $(navSelector).append('<hr />');
                        self.previous = $(navSelector).find('ul').last();
                        navigations = function ($el, obj) {
                            return self.navigations($el, obj);
                        };
                    }

                    // Build navigation & anchors
                    for (var i = 0; i < self.headers.length; i++) {
                        obj = self.headers.eq(i);
                        navigations($caller, obj);
                        self.anchor($caller, obj);
                    }

                    if ($caller.data(dataKey).spy) {
                        self.spy($caller);
                    }

                },
                navigations: function ($caller, obj) {

                    var self = this,
                        link,
                        list,
                        which,
                        name = self.name(obj);

                    if (obj.attr('id') !== undefined) {
                        name = obj.attr('id');
                    }

                    link = $('<a />')
                        .attr('href', '#' + name)
                        .addClass("text-muted")
                        .text(obj.text());

                    link.click(function (e) {
                        if ($caller.data(dataKey).onAnchorClick) {
                            $caller.data(dataKey).onAnchorClick($(this), e);
                        }
                    });

                    list = $('<li />').append(link);

                    which = parseInt(obj.prop('nodeName').substring(1), null);
                    list.attr('data-tag', which);

                    self.subheadings($caller, which, list);

                    self.first = which;
                },
                subheadings: function ($caller, which, a) {

                    var self = this,
                        navSelector = $caller.data(dataKey).navigation,
                        ul = $(navSelector).find('ul'),
                        li = $(navSelector).find('li');

                    if (which === self.first) {
                        self.previous.append(a);
                    } else if (which > self.first) {
                        li.last().append('<ul />');
                        // can't use cache ul; need to find ul once more
                        $(navSelector).find('ul').last().append(a);
                        self.previous = a.parent();
                    } else {
                        $('li[data-tag=' + which + ']').last().parent().append(a);
                        self.previous = a.parent();
                    }
                },
                name: function (obj) {
                    var name = obj.text().replace(/[^\w\s]/gi, '')
                        .replace(/\s+/g, '-')
                        .toLowerCase();
                    return name;
                },
                anchor: function ($caller, obj) {

                    var name = this.name(obj),
                        $anchor,                    
                        iconCss = $caller.data(dataKey).iconCss,                        
                        title = $caller.data(dataKey).anchorTitle,     
                        css = $caller.data(dataKey).anchorClass,
                        id;
           
                    if (obj.attr('id') === undefined) {
                        obj.attr('id', name);
                    }
                    id = obj.attr('id');

                    $anchor = $('<a />', {
                        "href": '#' + id,
                        "class": css
                    }).append($("<i />").addClass(iconCss));

                    if (title) {                       
                        $anchor.attr("title", title);
                    }

                    $anchor.click(function (e) {                        
                        if ($caller.data(dataKey).onAnchorClick) {
                            $caller.data(dataKey).onAnchorClick($(this), e);
                        }
                    });

                    if ($caller.data(dataKey).position === 'append') {
                        obj.append($anchor);
                    } else {
                        obj.prepend($anchor);
                    }

                },
                spy: function ($caller) {

                    var self = this,
                        previous,
                        current,
                        list,
                        top,
                        prev;

                    $(win).scroll(function (e) {

                        // get the header on top of the viewport
                        current = self.headers.map(function (e) {
                            if ($(this).offset().top - $(win).scrollTop() < $caller.data(dataKey).spyOffset) {
                                return this;
                            }
                        });

                        // get only the latest header on the viewport
                        current = $(current).eq(current.length - 1);

                        if (current && current.length) {
                            // get all li tag that contains href of # ( all the parents )
                            list = $('li:has(a[href="#' + current.attr('id') + '"])');

                            if (prev !== undefined) {
                                prev.removeClass('active');
                            }

                            list.addClass('active');
                            prev = list;
                        }
                    });
                }
            };

        return {
            init: function () {

                var options = {},
                    methodName = null,
                    func = null;          
                for (var i = 0; i < arguments.length; ++i) {
                    var a = arguments[i];
                    switch (a.constructor) {
                        case Object:
                            $.extend(options, a);
                            break;
                        case String:
                            methodName = a;
                            break;                     
                        case Function:
                            func = a;
                            break;
                    }
                }

                if (this.length > 0) {
                    // $(selector).anchorific()
                    return this.each(function () {
                        if (!$(this).data(dataIdKey)) {
                            var id = dataKey + parseInt(Math.random() * 100) + new Date().getTime();
                            $(this).data(dataIdKey, id);
                            $(this).data(dataKey, $.extend({}, defaults, options));
                        } else {
                            $(this).data(dataKey, $.extend({}, $(this).data(dataKey), options));
                        }
                        methods.init($(this), methodName, func);
                    });
                } else {
                    // $().anchorific()
                    var $caller = $(".entity-body");
                    if ($caller.length > 0) {
                        if (!$caller.data(dataIdKey)) {
                            var id = dataKey + parseInt(Math.random() * 100) + new Date().getTime();
                            $caller.data(dataIdKey, id);
                            $caller.data(dataKey, $.extend({}, defaults, options));
                        } else {
                            $caller.data(dataKey, $.extend({}, $caller.data(dataKey), options));
                        }
                        methods.init($caller, methodName, func);
                    }

                }

            }
        };

    }();

    // ---------------

    $.fn.extend({
        anchorific: anchorific.init
    });

    // ---------------

    var app = win.$.Plato;        

    app.ready(function () {
        
        var offset = $().layout("getHeaderHeight"),
            $header = null,
            opts = {
                title: app.T("Contents"),
                anchorTitle: app.T("Link to this section"),
                spyOffset: offset, // specify heading offset for spy scrolling                
                onAnchorClick: function ($link, e) {

                    // Prevent defaults
                    e.preventDefault();
                    e.stopPropagation();

                    var href = $link.attr("href");
                    $header = $(href);

                    // Set clicked anchor for onScrollEnd event               

                    // Scroll to anchor
                    $().scrollTo({
                        offset: -offset,
                        target: $header
                    }, "go");

                }
            };

        

        // Apply anchorific only to entity bodies
        $(".entity-body").anchorific(opts);

        // Update state if anchor was clicked after scrollEnd event
        $().infiniteScroll("scrollEnd", function () {

            // Ensure we have a clicked anchor
            if (!$header) {
                return;
            }

            var offsetString = "",
                infiniteScrollUrl = null,
                $infiniteScroll = $header.closest('[data-provide="infiniteScroll"]'),
                $offset = $header.closest(".card").find('[data-infinite-scroll-offset]'),
                offset = parseInt($offset.data("infiniteScrollOffset"));

            if (!isNaN(offset)) {
                offsetString = "/" + offset.toString();
            }

            if ($infiniteScroll.length > 0) {
                if ($infiniteScroll.data("infiniteScrollUrl")) {
                    infiniteScrollUrl = $infiniteScroll.data("infiniteScrollUrl");
                }
            }

            // Get url minus any existing anchor
            var url = "";
            if (infiniteScrollUrl) {
                url = infiniteScrollUrl + offsetString;
            } else {
                url = win.location.href.split("#")[0];
            }

            var hash = "";
            if ($header.attr("id")) {
                hash = "#" + $header.attr("id");
            }
            
            // Replace state
            if (url !== "") {           
                win.history.replaceState(win.history.state || {}, doc.title, url + hash);
            }

            $header = null;

        });

        // Activate anchorific when loaded via infiniteScroll load
        $().infiniteScroll("ready", function ($ele) {
            //$ele.find('[data-provide="markdownBody"]').anchorific(opts);
        });

    });

})(jQuery, window, document);
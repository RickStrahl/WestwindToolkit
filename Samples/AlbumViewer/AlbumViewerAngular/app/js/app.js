(function() {
    'use strict';

    var app = angular.module('app', [
        // Angular modules 
        'ngAnimate',
        'ngRoute',
        'ngSanitize',
        'ngTouch'
    ]);

    console.log('app starting', navigator.userAgent);

    // config settings
    app.configuration = {
        useLocalData: false
    };

    app.config([
            '$routeProvider',
            '$compileProvider',
            function($routeProvider, $compileProvider) {
                $routeProvider
                    .when("/albums", {
                        templateUrl: "app/views/albums.html"
                    })
                    .when("/album/edit/:albumId", {
                        templateUrl: "app/views/editalbum.html"
                    })
                    .when("/album/:albumId", {
                        templateUrl: "app/views/album.html"
                    })
                    .when("/artists", {
                        templateUrl: "app/views/artists.html"
                    })
                    .when("/artist/:artistId", {
                        templateUrl: "app/views/artist.html"
                    })
                    .when("/about", {
                        templateUrl: "app/views/about.html"
                    })
                    .when("/options", {
                        templateUrl: "app/views/options.html"
                    })
                    .otherwise({
                        redirectTo: '/albums'
                    });

                // Fix bug for Windows Phone wanting to download files on urls with routed parameters
                $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|file|ghttps?|ms-appx|x-wmapp0):/);
            }
        ])
        .filter('linebreakFilter', function() {
            return function(text) {
                if (text !== undefined)
                    return text.replace(/\n/g, '<br />');
                return text;
            };
        });

})();
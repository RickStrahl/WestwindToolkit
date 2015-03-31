(function () {
    'use strict';

    var app = angular
        .module('app')
        .controller('artistController', artistController);

    if (app.configuration.useLocalData)
        artistController.$inject = ["$http", "$window", "$routeParams", "$animate", "artistServiceLocal","albumServiceLocal"];
    else
        artistController.$inject = ["$http", "$window", "$routeParams", "$animate", "artistService","albumService"];

    function artistController($http,$window,$routeParams,$animate,artistService,albumService) {
        var vm = this;

        vm.artist = null;
        vm.artists = [];

        vm.error = {
            message: null,
            icon: "warning",
            reset: function() { vm.error = { message: "", icon: "warning" } },
            show: function(msg, icon) {
                vm.error.message = msg;
                vm.error.icon = icon ? icon : "warning";
            }
        };

        vm.getArtist = function (id) {
            artistService.getArtist(id)
                .success(function(response) {
                    vm.artist = response.Artist;
                    vm.albums = response.Albums;
                })
                .error(function() {
                    vm.error.show("Artist couldn't be loaded.", "warning");
                });
        };

        vm.saveArtist = function (artist) {            
            artistService.saveArtist(artist)
                .success(function (response) {                    
                    vm.artist = response.Artist;
                    vm.albums = response.Albums;

                    $("#EditModal").modal("hide");
                })
                .error(parseError);
        }

        vm.albumClick = function(album) {
            $window.location.hash = "/album/" + album.Id;
        };

        vm.addAlbum = function () {            
            albumService.album = albumService.newAlbum();
            albumService.album.ArtistId = vm.artist.Id;
            albumService.album.Artist = vm.artist;

            albumService.updateAlbum(albumService.album);
            $window.location.hash = "/album/edit/0";
        };

        vm.deleteArtist = function (artist) {
            artistService.deleteArtist(artist)
                .success(function(result) {
                    $window.location.hash = "/artists";
                })
                .error(parseError);
        }

        function parseError() {
            var err = ww.angular.parseHttpError(arguments);
            vm.error.show(err.message, "warning");
        }

        vm.getArtist($routeParams.artistId);

        // force explicit animation of the view and edit forms always
        //$animate.addClass("#MainView", "slide-animation");
    }
})();

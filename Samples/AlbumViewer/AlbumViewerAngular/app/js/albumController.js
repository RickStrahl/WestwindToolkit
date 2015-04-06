/// <reference path="albumService.js" />
/// <reference path="errorService.js" />
(function () {
    'use strict';

    var app = angular
        .module('app')
        .controller('albumController', albumController);


    var albumServiceString = "albumService";
    if (app.configuration.useLocalData)
        albumServiceString = "albumServiceLocal";

    //    albumController.$inject = ['$routeParams', '$window', '$animate','$location', 'albumService','errorService'];
    //else

    albumController.$inject = ['$routeParams', '$window', '$animate', '$location', albumServiceString, 'errorService'];

    function albumController($routeParams,$window,$animate,$location,albumService,errorService) {        
        var vm = this;
        
        vm.album = null;
        vm.selectedArtist = { ArtistName: null, Description: null };
        vm.error = errorService.error;

        vm.isSongVisible = false;
        vm.song = {
            Id: 0,
            AlbumId: 0,
            Name: null,
            Length: null
        };
 
        vm.saveAlbum = function (album) {
            console.log(album);
            
            albumService.saveAlbum(album)
                .success(function(album) {
                    vm.error.message = "Album saved";
                    vm.error.icon = "info";
                    setTimeout(function() {
                        $window.location.hash = "/album/" + album.Id;
                    },1000);
                })
                .error(function() {
                    vm.error.message = "Album not saved";
                    vm.error.icon = "warning";
                });
        };
        vm.addSong = function () {
            vm.isSongVisible = true;            
            vm.song = { Id: 0, AlbumId: albumService.album.Id, Name: null, Length: null };
            setTimeout(function() { $("#SongName").focus(); },300);
        };
        vm.saveSong = function (song) {            
            albumService.addSongToAlbum(vm.album, song);
            vm.albums = albumService.albums;
            vm.album = albumService.album;
            
            vm.isSongVisible = false;
        };
        vm.cancelSong = function() {
            vm.isSongVisible = false;
        };
        vm.removeSong = function (song) {            
            albumService.removeSong(vm.album, song);            
            vm.album = albumService.album;            
        };
        vm.deleteAlbum = function (album) {
            // on purpose! - to minimize vandalization by requiring popup
            if (!confirm("Are you sure you want to delete this album?"))
                return;

            albumService.deleteAlbum(album)
                .success(function () {
                    debugger;
                    vm.albums = albumService.albums;
                    $location.path("#/albums");
                })
                .error(function() {
                vm.error.message = "Album not deleted"
            });
        };
        vm.getAlbum = function(id) {            
            albumService.getAlbum(id, true)
            .success(function (album) {                
                vm.album = album;
            });
            
        }
        vm.bandTypeAhead = function() {
            var $input = $('#BandName');
            
            $input.typeahead({
                source: [],
                autoselect: true
            });
            $input.keyup(function () {                
                var s = $(this).val();
                $.getJSON("./artistlookup?search=" + s,
                    function (data) {
                        console.log(data);
                        $input.data('typeahead').source = data;
                    });
            });
        }
                
        // Initialization code
        var albumId = $routeParams.albumId * 1;
        if (albumId > 0)
            vm.getAlbum(albumId, true);
        else
            vm.album = albumService.album;

        // set up the type ahead control
        vm.bandTypeAhead();
        vm.error.reset();

        // force explicit animation of the view and edit forms always
        //$animate.addClass("#MainView","slide-animation");
    }
})();

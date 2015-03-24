(function () {
    'use strict';

    angular
        .module('app')
        .factory('artistServiceLocal', artistServiceLocal);

    artistServiceLocal.$inject = ['$http','$q'];


    function artistServiceLocal($http, $q) {
        var service;

        function newArtist() {
            return {
                Id: 0,
                ArtistName: null,
                AmazonUrl: null,
                AlbumCount: 0
            };
        }

        /// noCache: Not passed or 0: allow cache, 
        //          1 - no cache from memory,
        //          2 - re-read from disk
        function getArtists(noCache) {
            noCache = noCache || 0;

            // if albums exist just return
            if (!noCache && service.artists && service.artists.length > 0)
                return ww.angular.$httpPromiseFromValue($q, service.artists);

            if (noCache != 2) {
                // read from localstorage first
                var data = localStorage.getItem(service.lsArtists);
                if (data && data.length > 2) {
                    service.artists = JSON.parse(data);
                    return ww.angular.$httpPromiseFromValue($q, service.artists);
                }
            }

            return $http.get(service.baseUrl + "artists.js")
                .success(function(data) {
                    service.artists = data;
                    saveArtistList();
                })
                .error(onPageError);
        }

        function getArtist(id, useExisting) {
            if (id === 0 || id === '0') {
                service.artist = service.newArtist();
                return ww.angular.$httpPromiseFromValue($q, service.album);
            } else if (id === -1 || id === '-1' || !id)
                return ww.angular.$httpPromiseFromValue($q, service.album);

            // if the album is already loaded just return it
            // and return the promise
            if (service.artist && useExisting && service.artist.id == id)
                return ww.angular.$httpPromiseFromValue($q, service.artist);

            // ensure that albums exist - if not load those first and defer
            if (service.artists && service.artists.length > 0) {
                // just look up from cached list
                var artist = findArtist(id);
                if (!artist)
                    return ww.angular.$httpPromiseFromValue($q, new Error("Couldn't find artist"), true);
            }

            // otherwise load albums first
            var d = ww.angular.$httpDeferredExtender($q.defer());
            service.getArtists()
                .success(function(artists) {
                    service.artist = findArtist(id);
                    if (!service.artist)
                        d.reject(new Error("Couldn't find artist"));
                    else
                        d.resolve(service.artist);
                })
                .error(function(err) {
                    d.reject(new Error("Couldn't find artist"));
                });
            return d.promise;


            return ww.angular.$httpPromiseFromValue($q, service.artist);
        }


        function updateArtist(artist) {
            var i = findArtistIndex(artist);
            if (i > -1)
                service.artists[i] = artist;
            else {
                service.artists.push(artist);

                // remove pk of 0 from list if any
                service.artists = _.remove(service.artist, function(art) {
                    return artist.Id == 0;
                });
            }

            service.artist = artist;
        }

        function saveArtist(artist) {            
            // update the service list
            service.updateArtist(artist);
            service.artist = artist;
            saveArtistList();
            return ww.angular.$httpPromiseFromValue($q,service.artist);
        }

        function saveArtistList() {
            localStorage.setItem(service.lsArtists,JSON.stringify(service.artists));
        }

        function deleteArtist(artist) {
            return $http.get(service.baseUrl + "deletealbum/" + artist.Id)
                .success(function() {
                    service.albums = _.remove(service.albums, function(alb) {
                        return artist.Id != alb.Id;
                    });
                });
        }

        function findArtistIndex(artist) {
            return _.findIndex(service.artists, function(a) {
                if (typeof artist == "object")
                    return artist.Id == a.Id;
                else
                    return artist == a.Id;
            });
        }

        function findArtist(id) {
            id = id * 1;
            var a = _.find(service.artists, function(a) {
                console.log(id, a.Id);
                return id === a.Id;
            });
            return a;
        }

        service = {
            baseUrl: "data/",
            // local storage key
            lsArtists: "av_artists",
            artist: newArtist(),
            artists: [],
            getArtists: getArtists,
            getArtist: getArtist,
            updateArtist: updateArtist,
            saveArtist: saveArtist,
            deleteArtist: deleteArtist,
            newArtist: newArtist
        };
        return service;
    }
})();

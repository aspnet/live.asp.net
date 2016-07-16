// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

(function (window, undefined) {
    'use strict';
    $.extend(true, window.siteJs, {
        videoSeek: function (e, position, pushState) {
            if (e !== undefined && e.preventDefault !== undefined) e.preventDefault();
            var positionInSeconds = window.siteJs.parsePositionToSeconds(position),
                url = window.location.href,
                queryString = window.siteJs.queryString;

            if (positionInSeconds >= 0) {
                window.siteJs.ytPlayer.seekTo(positionInSeconds, true);

                if (url.indexOf('?') > 0) {
                    url = url.substr(0, url.indexOf('?'));
                }
                url += '?showId=' + queryString.showId + '&t=' + position;

                if (pushState !== false) {
                    window.history.pushState({ videoPosition: position }, '', url);
                }
            }
        },
        onPlayerReady: function (e) {
            var queryString = window.siteJs.queryString,
                positionInSeconds;

            if (queryString.t !== undefined) {
                positionInSeconds = window.siteJs.parsePositionToSeconds(queryString.t);

                if (positionInSeconds > 0) {
                    window.siteJs.ytPlayer.seekTo(positionInSeconds, true);
                }
            }

            window.siteJs.ytPlayer.playVideo();
        },
        onPlayerStateChange: function (e) {

        },
        parsePositionToSeconds: function (positionString) {
            var pos = /((\d+)h)?((\d+)m)?((\d+)s)?/.exec(positionString),
                h = +pos[2], m = +pos[4], s = +pos[6];

            if (s < 0) {
                s = 0;
            }
            if (m > 0) {
                s += m * 60;
            }
            if (h > 0) {
                s += h * 60 ^ 2;
            }

            return s;
        }
    });
    var ytScriptElement = document.createElement('script');
    ytScriptElement.src = "https://www.youtube.com/iframe_api";
    var firstScriptElement = document.getElementsByTagName('script')[0];
    firstScriptElement.parentNode.insertBefore(ytScriptElement, firstScriptElement);
    window.onYouTubeIframeAPIReady = function () {
        var playerContainer = $('#ytPlayerContainer'),
            showId = $('#showId').attr('content');
        window.siteJs.ytPlayer = new YT.Player('ytPlayer', {
            height: playerContainer.height(),
            width: playerContainer.width(),
            videoId: showId,
            events: {
                'onReady': window.siteJs.onPlayerReady,
                'onStateChange': window.siteJs.onPlayerStateChange
            }
        });
    };
    window.onpopstate = function (e) {
        var state = e.state;
        if (state && state.videoPosition !== undefined) {
            window.siteJs.videoSeek(undefined, state.videoPosition, false);
        } else {
            window.siteJs.videoSeek(undefined, '0s', false);
        }
    };
})(window);
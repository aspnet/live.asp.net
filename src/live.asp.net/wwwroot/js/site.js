// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

(function (window, undefined) {
    'use strict';

    var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

    Math.trunc = Math.trunc || function (x) {
        return x < 0 ? Math.ceil(x) : Math.floor(x);
    };

    function monthName(month) {
        return months[month];
    }

    function formatTime(hours, minutes) {
        var tt = hours < 12 ? ' AM' : ' PM',
            hour = hours < 12 ? hours
                : hours === 12 ? 12
                    : (hours - 12),
            mins = minutes < 10 ? '0' + minutes : minutes;

        return hour + ':' + mins + tt;
    }

    function formatDateTime(dateTime) {
        return dateTime.getDate() + ' ' +
               monthName(dateTime.getMonth()) + ', ' +
               dateTime.getFullYear() + ' at ' +
               formatTime(dateTime.getHours(), dateTime.getMinutes());
    }

    function extractTimeZoneName(dateString) {
        var name = dateString.substring(dateString.indexOf('(') + 1);
        name = name.substr(0, name.length - 1);
        return name;
    }

    function dateDiff(date1, date2) {
        var ms = date2 - date1,
            totalSecs, totalMins, totalHrs, totalDays, totalWeeks,
            weeks, days, hrs, mins, secs;

        totalSecs = ms / 1000;
        totalMins = totalSecs / 60;
        totalHrs = totalMins / 60;
        totalDays = totalHrs / 24;
        totalWeeks = totalDays / 7;

        weeks = Math.trunc(totalWeeks);
        days = Math.trunc(totalDays - (weeks * 7));
        hrs = Math.trunc(totalHrs - (days * 24) - (weeks * 7 * 24));
        mins = Math.trunc(totalMins - (hrs * 60) - (days * 24 * 60) - (weeks * 7 * 24 * 60));
        /* jshint -W101 */
        secs = Math.trunc(totalSecs - (mins * 60) - (hrs * 60 * 60) - (days * 24 * 60 * 60) - (weeks * 7 * 24 * 60 * 60));

        return {
            secs: secs,
            mins: mins,
            hrs: hrs,
            days: days,
            weeks: weeks,
            totalSecs: totalSecs,
            totalMins: totalMins,
            totalHrs: totalHrs,
            totalDays: totalDays,
            totalWeeks: totalWeeks,
            toString: function () {
                return weeks + ' weeks, ' + days + ' days, ' + hrs + ' hours, ' + mins + ' mins, ' + secs + ' secs';
            },
            toShortString: function () {
                return (weeks > 0 ? weeks + 'w ' : '') +
                       (days > 0 ? days + 'd ' : '') +
                       (hrs > 0 ? hrs + 'h ' : '') +
                       (mins > 0 ? mins + 'm ' : '') +
                       secs + 's';
            }
        };
    }

    function countdownTo(futureDate, tickCallback, endCallback) {
        var interval = window.setInterval(function () {
            var now = new Date(),
                diff = dateDiff(now, futureDate);

            if (diff.totalSecs < 0) {
                //window.console.log('Clearing interval');
                window.clearInterval(interval);
                endCallback();
                return;
            }

            tickCallback(diff);
        }, 500);
    }

    function data(el, name) {
        return el.getAttribute('data-' + name);
    }

    function getNextShowTime(el) {
        return new Date(Date.UTC(
            data(el, 'utc-year'),
            data(el, 'utc-month'),
            data(el, 'utc-day'),
            data(el, 'utc-hour'),
            data(el, 'utc-min'),
            data(el, 'utc-sec')));
    }

    window.siteJs = {
        setNextShowDetails: function (elementId) {
            // Get the show details
            var countdownEl,
                showDetailsEl = window.document.getElementById(elementId),
                showTimeUtc = getNextShowTime(showDetailsEl),

            // Set the show time display
            showTimeEl = showDetailsEl.querySelector('[data-part="showTime"]'),
            timeZoneName = extractTimeZoneName(showTimeUtc.toString());

            showTimeEl.textContent = formatDateTime(showTimeUtc) + ' (' + timeZoneName + ')';
            showTimeEl.className = '';

            // Start the countdown
            countdownEl = showDetailsEl.querySelector('[data-part="countdown"]');
            countdownTo(showTimeUtc, function (diff) {
                countdownEl.textContent = diff.toShortString();
            }, function () {
                countdownEl.textContent = '';
            });
        }
    };

})(window);

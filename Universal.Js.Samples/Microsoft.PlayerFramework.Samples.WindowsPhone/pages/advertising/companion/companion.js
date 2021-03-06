﻿(function () {
    "use strict";

    var mediaPlayer = null;

    var midrollAd = new PlayerFramework.Advertising.MidrollAdvertisement();
    midrollAd.source = new Microsoft.PlayerFramework.Js.Advertising.RemoteAdSource();
    midrollAd.source.type = Microsoft.Media.Advertising.VastAdPayloadHandler.adType;
    midrollAd.source.uri = new Windows.Foundation.Uri("http://smf.blob.core.windows.net/samples/win8/ads/vast_linear_companions.xml");
    midrollAd.time = 5;

    WinJS.UI.Pages.define("/pages/advertising/companion/companion.html", {
        // This function is called whenever a user navigates to this page.
        // It populates the page with data and initializes the media player control.
        ready: function (element, options) {
            var item = options && options.item ? Data.resolveItemReference(options.item) : Data.items.getAt(0);
            element.querySelector(".titlearea .pagetitle").textContent = item.title;
            if (WinJS.Utilities.isPhone) {
                document.getElementById("header").style.display = "none";
            }

            var mediaPlayerElement = element.querySelector("[data-win-control='PlayerFramework.MediaPlayer']");
            mediaPlayer = mediaPlayerElement.winControl;
            mediaPlayer.adSchedulerPlugin.advertisements.push(midrollAd);
            mediaPlayer.focus();
        },

        // This function is called whenever a user navigates away from this page.
        // It resets the page and disposes of the media player control.
        unload: function () {
            if (mediaPlayer) {
                mediaPlayer.dispose();
                mediaPlayer = null;
            }
        }
    });
})();
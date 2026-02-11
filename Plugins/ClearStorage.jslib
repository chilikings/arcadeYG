
// Assets/Plugins/ClearStorage.jslib
mergeInto(LibraryManager.library, {
    ReloadWebPage: function() {
        console.log('[jslib] ReloadWebPage called → reloading page');
        window.location.reload();
    },

    ClearCookies: function() {
        var cookies = document.cookie ? document.cookie.split(';') : [];
        console.log('[jslib] before clear, document.cookie =', document.cookie);
        console.log('[jslib] ClearCookies called → found ' + cookies.length + ' cookies');
        var hostname = window.location.hostname;
        cookies.forEach(function(cookie) {
            var name = cookie.split('=')[0].trim();
            console.log('[jslib] Clearing cookie:', name);
            document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/';
            document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/;domain=' + hostname;
        });
        console.log('[jslib] ClearCookies finished');
    },

    ClearPlayerPrefs: function() {
        console.log('[jslib] ClearPlayerPrefs called → deleting UnityPlayerPrefs DB');
        try {
            window.indexedDB.deleteDatabase('UnityPlayerPrefs');
            console.log('[jslib] IndexedDB.deleteDatabase("UnityPlayerPrefs") succeeded');
        } catch (e) {
            console.error('[jslib] IndexedDB.deleteDatabase error:', e);
        }
        try {
            window.localStorage.clear();
            console.log('[jslib] localStorage.clear() succeeded');
        } catch (e) {
            console.error('[jslib] localStorage.clear() error:', e);
        }
        console.log('[jslib] ClearPlayerPrefs finished');
    }
});

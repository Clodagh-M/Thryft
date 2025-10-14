// wwwroot/js/reloadDetector.js
window.reloadDetector = {
    isPageReloaded: function () {
        const performance = window.performance;
        if (performance) {
            const navigation = performance.getEntriesByType("navigation")[0];
            if (navigation) {
                return navigation.type === 'reload';
            }
        }

        // Fallback: check sessionStorage
        const wasReloaded = sessionStorage.getItem('pageWasReloaded') === 'true';
        sessionStorage.setItem('pageWasReloaded', 'true');
        return wasReloaded;
    },

    clearReloadFlag: function () {
        sessionStorage.removeItem('pageWasReloaded');
    }
};
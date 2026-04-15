// Auto-scroll du pattern editor vers la row active
window.ft2ScrollToActiveRow = function (containerId, rowIndex, rowHeight, totalRows) {
    var container = document.getElementById(containerId);
    if (!container) return;

    var containerHeight = container.clientHeight;
    var visibleRows = Math.floor(containerHeight / rowHeight);
    var centerOffset = Math.floor(visibleRows / 2);

    // Scroll pour garder la row active au centre
    var targetScroll = (rowIndex - centerOffset) * rowHeight;
    targetScroll = Math.max(0, Math.min(targetScroll, (totalRows * rowHeight) - containerHeight));

    container.scrollTop = targetScroll;
};

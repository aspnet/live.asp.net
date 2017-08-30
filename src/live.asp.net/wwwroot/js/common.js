$(function () {
    $("a").click(function () {
        if (typeof mscc !== 'undefined') {
            mscc.setConsent();
        }
    });
});

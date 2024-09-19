$(document).ready(function () {
    var resizing = false;

    $('#txtForeignText').on('mousedown', function (e) {
        if ($(e.target).is('#txtForeignText')) {
            resizing = true;
        }
    });

    $(document).on('mouseup', function () {
        if (resizing) {
            resizing = false;
            setTimeout(function () {
                var modalPopupBehavior = $find('mpeForeignTextForm');
                if (modalPopupBehavior) {
                    modalPopupBehavior._layout();
                }
            }, 50);
        }
    });

});

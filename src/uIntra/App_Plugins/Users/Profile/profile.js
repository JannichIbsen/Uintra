﻿import fileUploadController from "./../../Core/Controls/FileUpload/file-upload";
import ajax from "./../../Core/Content/scripts/Ajax";
import confirm from "./../../Core/Controls/Confirm/Confirm";

require("./profile.css");

var initDeleteButton = function (holder) {    
    var btn = holder.find('.js-delete-btn');  

    btn.click(function () { 
        var confirmMessage = btn.data('confirm-message');
        confirm.showConfirm(confirmMessage, 
            function () {
                ajax.Delete("/umbraco/surface/Profile/DeletePhoto").then(function(response) {
                    location.reload();
                });
            }, function () { }, confirm.defaultSettings);
    });
}

function initListeners() {
    $('#js-member-notifier-setting').on('change', function (event) {

        let $this = $(this);
        let element = event.currentTarget;
        let notifierType = element.attributes.notifiertype.value;
        let value = element.checked;
        $.ajax({
            type: "POST",
            data: { id: $this.data("id") },
            url: "/umbraco/api/MemberNotifierSettings/Update?type=" + notifierType + "&isEnabled=" + value
        });
    });
}


var controller = {
    init: function () {
        var holder = $('#js-profile-page');
        if (!holder.length) {
            return;
        }
        initListeners();
        initDeleteButton(holder);
        fileUploadController.init(holder);
    }
}

export default controller;
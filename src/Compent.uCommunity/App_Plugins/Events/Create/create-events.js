﻿require("./../../Core/Content/libs/jquery.validate.min.js");
require("./../../Core/Content/libs/jquery.unobtrusive-ajax.min.js");
require("./../../Core/Content/libs/jquery.validate.unobtrusive.min.js");

import appInitializer from "./../../Core/Content/scripts/AppInitializer";
import helpers from "./../../Core/Content/scripts/Helpers";
import fileUploadController from "./../../Core/Controls/FileUpload/file-upload";

require('select2');
require('./../../Core/Content/scripts/ValidationExtensions');

var holder;
var userSelect;
var editor;

var initPinControl=function() {    
    var pinControl = holder.find('#pin-control');
    var pinInfoHolder = holder.find('#pin-info');
    $(pinInfoHolder).hide();

    pinControl.change(function() {
        if ($(this).is(":checked")) {
            pinInfoHolder.show();
        } else {
            pinInfoHolder.hide();
        }
    });
}
var initUserSelect = function () {
    userSelect = holder.find('#js-user-select').select2({});
}

var initDescriptionControl = function () {
    var dataStorage = holder.find('#js-hidden-description-container');
    var descriptionElem = holder.find('#description');
    var btn = holder.find('.form__btn._submit');

    editor = helpers.initQuill(descriptionElem[0], dataStorage[0], { theme: 'snow' });

    editor.on('text-change', function () {
        if (editor.getLength() > 1 && descriptionElem.hasClass('input-validation-error')) {
            descriptionElem.removeClass('input-validation-error');
        }
    });

    btn.click(function () {
        editor.getLength() <= 1 ?
            descriptionElem.addClass('input-validation-error') :
            descriptionElem.removeClass('input-validation-error');
    });
}

var controller = {
    init: function () {
        holder = $('#js-events-create-page');

        if (!holder.length) {
            return;
        }

        initPinControl();
        initUserSelect();
        helpers.initDatePicker(holder, '#js-start-date', '#js-start-date-value');
        helpers.initDatePicker(holder, '#js-end-date', '#js-end-date-value');
        initDescriptionControl();
        fileUploadController.init(holder);
    }
}

appInitializer.add(controller.init);
import helpers from "./../Core/Content/scripts/Helpers";
import umbracoAjaxForm from "./../Core/Content/scripts/UmbracoAjaxForm";
import lightbox from "./../Core/Controls/LightboxGallery/LightboxGallery";
import subscribe from "./../Subscribe/subscribe";
import readonlyClickWarning from "./../Core/Content/scripts/readonlyClickWarning";
import feedStateService from './feedStateService'

import Vue from "vue";
import vueFilter from "./../../Content/vue/Filter/filter.vue";

var filterDomEl = document.getElementById("vue-filter");


if (filterDomEl) {
    new Vue({
        el: "#vue-filter",
        render: h => h(vueFilter)
    });
}

require("./centralFeed.css");

let infinityScroll = helpers.infiniteScrollFactory;
let scrollTo = helpers.scrollTo;

let holder;
let formController;

function initDescription() {
    const container = $("._clamp");
    if (container.length > 0) {
        for (let i = 0; i < container.length; i++) {
            const target = $(container[i]).data("url");
            helpers.clampText(container[i], target);
        }
    }
}

function showLoadingStatus() {
    const loadingElem = document.querySelector(".js-loading-status");
    loadingElem && (loadingElem.style.display = "block");
}

function hideLoadingStatus() {
    const loadingElem = document.querySelector(".js-loading-status");
    loadingElem && (loadingElem.style.display = "none");
}

function initCustomControls(data) {
    if (!data) {
        return;
    }

    lightbox.init();
    initDescription();
    subscribe.initOnLoad();
}

function scrollPrevented() {
    return !!parseInt(holder.querySelector('input[name="preventScrolling"]').value) | false;
}

function reload(skipLoadingStatus) {

    !skipLoadingStatus && showLoadingStatus();

    const promise = formController.reload();
    promise.then(hideLoadingStatus);
    promise.then(initCustomControls);
    promise.catch(hideLoadingStatus);
    return promise;
}

function scrollReload() {
    const promise = formController.reload();
    promise.then(attachReadonlyClickWarning);
    promise.then(initCustomControls);
    return promise;
}

function attachReadonlyClickWarning() {
    readonlyClickWarning.init();
}

function reloadTabEventHandler(e) {
    const hash = (window.location.hash || "").replace("#", "");

    reload(false, e.detail.isReinit).then(function () {
        if (hash) {
            const elem = document.querySelector('[data-anchor="' + hash + '"]');

            if (elem) {
                scrollTo(document.body, elem.offsetTop, 300);
                window.history.pushState("", document.title, window.location.pathname + window.location.search);
            }
        }
    });
}

function init() {
    holder = document.querySelector(".js-feed-overview");
    if (!holder) return;
    document.addEventListener(feedStateService.feedUpdateEventId, () => reload(false, false));
    formController = umbracoAjaxForm(holder.querySelector("form.js-ajax-form"));
    readonlyClickWarning.init();
    
    initDescription();

    infinityScroll({
        storageName: "infScroll",
        loaderSelector: ".js-loading-status",
        $container: $(formController.form),
        reload: scrollReload
    });
}

export default {
    init,
    reload
}
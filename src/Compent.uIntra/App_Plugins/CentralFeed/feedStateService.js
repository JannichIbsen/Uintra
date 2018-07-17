let state = {
    showFilters:false,
    showSubscribed:false,
    showPinned:false,
    includeBulletin:false,
    selectedActivityTypeId:1
};
let feedStateHubProxy = null;
let feedUpdateEventId = "feedUpdate";
let feedStateUpdateEventId = "feedStateUpdate";

function init() {
    const connection = $.hubConnection();
    feedStateHubProxy = connection.createHubProxy("feedStateHub");
    connection.start().done(()=>initEvents(feedStateHubProxy));
}

function initEvents(hub) {
    hub.on("filtersStateUpdate", function (newState) {
        state = newState;
        dispatchEvent(feedStateUpdateEventId, { state: state});
    });

    hub.on("feedUpdate", function () {
        debugger;
        dispatchEvent(feedUpdateEventId);
    });

    dispatchEvent(feedUpdateEventId);
}

function dispatchEvent(identifier, args) {
    document.dispatchEvent(new CustomEvent(identifier), args);
}

function saveState(newState) {
    console.log(newState);
    feedStateHubProxy.invoke("updateFiltersState", newState).done(function () {
        console.log('done');
    }).fail(function (error) {
        console.log('Error: ' + error);
    });
}

const feedStateService = {
    init,
    feedUpdateEventId,
    feedStateUpdateEventId,
    changeState: function (stateKey, value) {
        state[stateKey] = value;
        saveState(state);
    }
}

export default feedStateService;
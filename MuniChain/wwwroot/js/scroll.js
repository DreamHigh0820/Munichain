window.ScrollToBottom = (elementId) => {
    element = document.getElementById(elementId);
    element.scrollTop = element.scrollHeight - element.clientHeight;
}
$("#searchbox").keyup(function (event) {
    if (event.keyCode === 13) {
        $("#searchbutton").click();
    };
});
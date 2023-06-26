var limitCharsInNumberFields = document.getElementsByClassName("length-limit");

Array.prototype.forEach.call(limitCharsInNumberFields, function forElement(field) {
    field.addEventListener("input", (e) => { 
        e.target.value=e.target.value.slice(0,e.target.maxLength) 
    })
});

var createTransfersPledgeButton = document.getElementById("CreateTransfersPledgeButton");

// Disable click action on the link if it is disabled
if (createTransfersPledgeButton.getAttribute("disabled")) {
    createTransfersPledgeButton.addEventListener("click", function (event) {
        event.preventDefault(); // Prevent the default action of the click event
    });
}
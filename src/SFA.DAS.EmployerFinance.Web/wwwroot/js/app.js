var limitCharsInNumberFields = document.getElementsByClassName("length-limit");

Array.prototype.forEach.call(limitCharsInNumberFields, function forElement(field) {
    field.addEventListener("input", (e) => { 
        e.target.value=e.target.value.slice(0,e.target.maxLength) 
    })
});


let createTransfersPledgeButton = document.getElementById("CreateTransfersPledgeButton");
createTransfersPledgeButton.addEventListener("click", function (event) {
    if (createTransfersPledgeButton.classList.contains("govuk-button--disabled")) {
        event.preventDefault();
    }
});
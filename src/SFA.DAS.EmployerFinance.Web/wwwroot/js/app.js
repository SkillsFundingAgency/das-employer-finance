var limitCharsInNumberFields = document.getElementsByClassName("length-limit");

Array.prototype.forEach.call(limitCharsInNumberFields, function forElement(field) {
    field.addEventListener("input", (e) => { 
        e.target.value=e.target.value.slice(0,e.target.maxLength) 
    })
});


const createTransfersPledgeButton = document.getElementById("CreateTransfersPledgeButton");

if (createTransfersPledgeButton) {
    createTransfersPledgeButton.addEventListener("click", function (event) {
        if (createTransfersPledgeButton.classList.contains("govuk-button--disabled")) {
            event.preventDefault();
        }
    });
}

const printLinks = document.querySelectorAll(
    ".actions__link--print"
);

if (printLinks.length > 0) {
    for (let i = 0; i < printLinks.length; i++) {
        printLinks[i].addEventListener("click", (e) => {
            e.preventDefault();
            window.print();
        });
    }
}
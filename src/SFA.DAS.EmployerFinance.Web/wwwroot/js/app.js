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
    ".app-actions__link--print"
);

if (printLinks.length > 0) {
    for (let i = 0; i < printLinks.length; i++) {
        printLinks[i].addEventListener("click", (e) => {
            e.preventDefault();
            window.print();
        });
    }
}

const financeEstimates = document.getElementById("finance-estimates");

if (financeEstimates) {
    const estimatesRefreshDelays = [2000, 5000, 10000, 15000, 20000];
    let estimatesRefreshAttempt = 0;

    const checkForUpdatedEstimates = () => {
        const url = new URL(financeEstimates.dataset.estimatesUrl, window.location.origin);
        url.searchParams.set("lastUpdated", financeEstimates.dataset.lastUpdated);

        fetch(url, { credentials: "same-origin", headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then((response) => {
                if (response.status === 200) {
                    return response.text().then((html) => {
                        financeEstimates.innerHTML = html;
                    });
                }

                if (response.status === 204) {
                    scheduleEstimatesRefresh();
                }
            })
            .catch(() => {
            });
    };

    const scheduleEstimatesRefresh = () => {
        if (estimatesRefreshAttempt < estimatesRefreshDelays.length) {
            setTimeout(checkForUpdatedEstimates, estimatesRefreshDelays[estimatesRefreshAttempt++]);
        }
    };

    scheduleEstimatesRefresh();
}

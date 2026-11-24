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
    const estimatesRefreshDelays = [200, 200, 200, 500, 1000, 2000, 5000, 10000, 15000];
    const estimatesRefreshPollDelay = 15000;
    const estimatesRefreshTimeout = 120000;
    const estimatesRefreshStarted = Date.now();
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

    // once the initial delays have been used up, keep checking at a steady interval until the estimates change,
    // giving up after the timeout so the page keeps the estimates it loaded with
    const scheduleEstimatesRefresh = () => {
        const delay = estimatesRefreshAttempt < estimatesRefreshDelays.length
            ? estimatesRefreshDelays[estimatesRefreshAttempt++]
            : estimatesRefreshPollDelay;

        if (Date.now() - estimatesRefreshStarted + delay > estimatesRefreshTimeout) {
            return;
        }

        setTimeout(checkForUpdatedEstimates, delay);
    };

    scheduleEstimatesRefresh();
}

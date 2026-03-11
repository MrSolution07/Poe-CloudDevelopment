(function () {
    'use strict';

    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            var closeBtn = alert.querySelector('.btn-close');
            if (closeBtn) closeBtn.click();
        }, 5000);
    });

    var validationSummary = document.querySelector('.validation-summary-errors, .text-danger');
    if (validationSummary && validationSummary.textContent.trim().length > 0) {
        validationSummary.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
})();

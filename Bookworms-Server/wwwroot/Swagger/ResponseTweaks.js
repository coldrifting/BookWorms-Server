function checkForAndHideNonJsonExamples() {
    document.querySelectorAll('.response').forEach(response => {
        const option = response.querySelector('option');
        const example = response.querySelector('.model-example');

        // Hide example responses for errors and non-JSON example responses 
        if (response.dataset.code.startsWith('4')
            || (option && option.textContent.trim() !== 'application/json')) {
            example.style.display = 'none';
        }
    });
}

// Run the function when the DOM loads the first time
document.addEventListener('DOMContentLoaded', function() {
    checkForAndHideNonJsonExamples();

    // Observe changes in the DOM; run the function again if any changes occur
    const observer = new MutationObserver(checkForAndHideNonJsonExamples);
    observer.observe(document.body, { childList: true, subtree: true });
});
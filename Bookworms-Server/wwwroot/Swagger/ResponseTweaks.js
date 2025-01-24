function checkForAndHideNonJsonExamples() {
    document.querySelectorAll('.response').forEach(response => {
        const desc = response.querySelector('.renderedMarkdown')
        const code = response.querySelector('.response-col_status')
        if (code) {
            if (code.innerHTML.startsWith('4')) {
                code.style.color = 'var(--delete-method-color)';
                if (desc) {
                    desc.style.color = code.style.color;
                }
                
                const example = response.querySelector('.model-example');
                if (example) {
                    example.style.display = 'none';
                }
            }
            else {
                code.style.color = 'var(--primary-text-color)';
                if (desc) {
                    desc.style.color = code.style.color;
                }
            }
        }
        
        const controls = response.querySelector('.response-controls');
        if (controls) {
            controls.style.display = 'none';
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
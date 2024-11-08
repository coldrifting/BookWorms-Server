function waitForElm(selector) {
    return new Promise(resolve => {
        if (document.querySelector(selector)) {
            return resolve(document.querySelector(selector));
        }

        const observer = new MutationObserver(mutations => {
            if (document.querySelector(selector)) {
                observer.disconnect();
                resolve(document.querySelector(selector));
            }
        });

        // If you get "parameter 1 is not of type 'Node'" error, see https://stackoverflow.com/a/77855838/492336
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    });
}

var callback = function() {
    // Move authorize button to header
    waitForElm('.topbar-wrapper').then((elm) => {
        console.log('Element is ready');
        console.log(elm);
        
        waitForElm('.auth-wrapper').then((elm2) => {
            console.log('Element is ready');
            console.log(elm2);
            
            elm.appendChild(elm2);
        });
        
    });
    
    
    /*
    const x = document.getElementsByClassName('topbar-wrapper').item(0);
    
    var btn = document.createElement("div");
    btn.innerHTML = 
        "<div class='auth-wrapper'><button class='btn authorize unlocked'><span>Authorize</span><svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20' width='20' height='20' aria-hidden='true' focusable='false'><path d='M15.8 8H14V5.6C14 2.703 12.665 1 10 1 7.334 1 6 2.703 6 5.6V6h2v-.801C8 3.754 8.797 3 10 3c1.203 0 2 .754 2 2.199V8H4c-.553 0-1 .646-1 1.199V17c0 .549.428 1.139.951 1.307l1.197.387C5.672 18.861 6.55 19 7.1 19h5.8c.549 0 1.428-.139 1.951-.307l1.196-.387c.524-.167.953-.757.953-1.306V9.199C17 8.646 16.352 8 15.8 8z'></path></svg></button></div>"
    x.appendChild(btn);
    
    const authBtn = document.getElementsByClassName("auth-wrapper").item(0);
    
    console.log(authBtn);
    
    if (authBtn) {
        authBtn.parentElement.backgroundColor = "#ffffff";
    }
    
    var elem = document.createElement("div");
    elem.innerHTML =
        "<div style=\"text-align: center; font-family: Titillium Web,sans-serif; margin: 16px;\">This text was injected via /ext/custom-javascript.js, using the SwaggerUIOptions.InjectJavascript method.</div>";

    document.body.insertBefore(elem, document.body.firstChild);
    
     */
};

if (document.readyState === "complete" || (document.readyState !== "loading" && !document.documentElement.doScroll)) {
    callback();
} else {
    document.addEventListener("DOMContentLoaded", callback);
}
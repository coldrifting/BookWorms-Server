function waitForElm(selector) {
    return new Promise(resolve => {
        if (document.querySelector(selector)) {
            return resolve(document.querySelector(selector));
        }

        const observer = new MutationObserver(() => {
            if (document.querySelector(selector)) {
                //observer.disconnect();
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

function SetLoginVisibility(btn, invert = false) {
    const currentTokenVal = document.getElementById('auth-bearer-value');
    if (currentTokenVal) {
        console.log(currentTokenVal);
        if (currentTokenVal.value === '') {
            return;
        }
    }

    let isLogout = btn.innerHTML !== 'Logout';
    if (invert) {
        isLogout = !isLogout;
    }

    const username = document.getElementById('username');
    if (username) {
        document.getElementById('username').disabled = isLogout;
    }
    const password = document.getElementById('password');
    if (password) {
        password.disabled = isLogout;
    }

    const getTokenBtn = document.getElementById('getTokenBtn');
    if (getTokenBtn) {
        getTokenBtn.disabled = isLogout;
    }
}

// Sets up the username & password part of auth panel
function AuthPanel() {
    const elm = document.getElementsByClassName('auth-container').item(0);

    if (!elm || elm.childElementCount >= 2) {
        return;
    }

    elm.parentElement.classList.add('auth-box');
    elm.classList.add('auth-box');

    const form = document.getElementsByClassName('auth-container').item(0).firstChild;
    form.style.paddingTop = "1em";

    const credDiv = document.createElement('form');
    credDiv.classList.add('cred-div');

    const userInput = document.createElement('div');
    userInput.classList.add('wrapper');
    userInput.innerHTML = '<label for="username">Username:</label><section class=""><input name="username" id="username" type="text" aria-label="username" class="cred-div-input"></section>';

    const passwordInput = document.createElement('div');
    passwordInput.classList.add('wrapper');
    passwordInput.innerHTML = '<label for="password">Password:</label><section class=""><input name="password" id="password" type="password" aria-label="password" class="cred-div-input"></section>';

    const getTokenBtn = document.createElement('div');
    getTokenBtn.classList.add('wrapper', 'button-padding');
    getTokenBtn.innerHTML = '<button type="submit" class="btn modal-btn auth authorize button button-get" id="getTokenBtn" aria-label="Get credentials">Get API Token</button>';

    const authErrors = document.createElement('div');
    authErrors.classList.add("error-hint", "errors", "hidden");
    authErrors.innerHTML =
        '<b>Error:</b><span>Unauthorized</span>';

    credDiv.append(userInput);
    credDiv.append(passwordInput);
    credDiv.append(getTokenBtn);
    credDiv.append(authErrors);

    form.parentElement.style.display = "flex";
    form.parentElement.insertBefore(credDiv, form);

    const username = document.getElementById('username');
    if (username) {
        username.focus();
    }

    waitForElm('.auth-btn-wrapper').then(e => {
        const btn = e.firstElementChild;
        if (btn) {
            SetLoginVisibility(btn, true);
            btn.addEventListener('click', () => {
                SetLoginVisibility(btn);
            })
        }
    });

    credDiv.addEventListener("submit", e => {
        e.preventDefault();

        const formData = new FormData(credDiv);
        const username = formData.get("username");
        const password = formData.get("password");

        if (username === "" || password === "") {
            return;
        }

        // Get Token from login API
        fetch("/account/login", {
            method: "POST",
            headers: {
                "accept": "application/json",
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                "email": username,
                "password": password
            })
        })
            .then((response) => {
                // Handle the response from the server
                if (response.ok) {
                    console.log('ok');
                    console.log(authErrors);
                    authErrors.classList.add("hidden");

                    response.json().then(x => {
                        const tokenInput = document.getElementById('auth-bearer-value');

                        if (!tokenInput) {
                            return;
                        }

                        tokenInput.setAttribute("value", x['accessToken']);
                        tokenInput.dispatchEvent(new Event('change', {bubbles: true}));

                        document.getElementById('username').disabled = true;
                        document.getElementById('password').disabled = true;
                        document.getElementById('getTokenBtn').disabled = true;

                        form.requestSubmit();
                    });
                } else {
                    console.log('not ok');
                    console.log(authErrors);
                    authErrors.classList.remove("hidden");
                }
            })
            .catch((error) => {
                // Handle any errors
                console.log(error);
                authErrors.classList.remove("hidden");
            });
    });
}

const callback = function () {
    // Move authorize button to header
    waitForElm('.topbar-wrapper').then((elm) => {
        waitForElm('.auth-wrapper').then((elm2) => {
            elm.appendChild(elm2);
            elm2.firstChild.style.display = "flex";
        });
    });

    // Make sure we reapply the auth panel tweaks each time it (re)appears
    waitForElm('.auth-wrapper').then(e => {
        new MutationObserver((mutationList, _) => {
            for (const mutation of mutationList) {
                if (mutation.type === "childList") {
                    AuthPanel();
                }
            }
        }).observe(e, {attributes: false, childList: true, subtree: true});
    });
};


if (document.readyState === "complete" || (document.readyState !== "loading" && !document.documentElement.doScroll)) {
    callback();
} else {
    document.addEventListener("DOMContentLoaded", callback);
}
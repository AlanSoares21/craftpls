// Write your JavaScript code.

function updateCookies() {
    updateTimeZone();
    updateCulture();
}

function updateTimeZone() {
    if (!document.cookie.includes('UserTimezoneId'))
        document.cookie = `UserTimezoneId=${Intl.DateTimeFormat().resolvedOptions().timeZone};SameSite=none;Path=/`
}

function updateCulture() {
    if (!document.cookie.includes('Culture')) {
        document.cookie = `Culture=${navigator.language.split('-')[0]};SameSite=none;Path=/`
        location.href = location.href
    }
}

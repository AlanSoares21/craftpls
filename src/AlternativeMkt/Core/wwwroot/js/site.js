// Write your JavaScript code.

function updateTimeZone() {
    document.cookie = `UserTimezoneId=${Intl.DateTimeFormat().resolvedOptions().timeZone};SameSite=none;Path=/`
}

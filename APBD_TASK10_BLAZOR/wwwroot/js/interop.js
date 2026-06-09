export function copyToClipboard(text) {
    if (navigator.clipboard && window.isSecureContext) {
        return navigator.clipboard.writeText(text).then(() => true).catch(() => false);
    }

    try {
        const area = document.createElement("textarea");
        area.value = text;
        area.style.position = "fixed";
        area.style.opacity = "0";
        document.body.appendChild(area);
        area.select();
        const ok = document.execCommand("copy");
        document.body.removeChild(area);
        return Promise.resolve(ok);
    } catch {
        return Promise.resolve(false);
    }
}

export function confirmDialog(message) {
    return window.confirm(message);
}

export function saveLastSemester(value) {
    localStorage.setItem("apbd.lastSemester", value);
}

export function getLastSemester() {
    return localStorage.getItem("apbd.lastSemester");
}

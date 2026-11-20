// Keep custom Blazor dialogs keyboard-accessible without changing their actions.
(() => {
    const focusableSelector = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
    const openers = new Map();
    let previousOverflow = '';
    let previousDialogs = [];
    const getDialogs = () => [...document.querySelectorAll('[role="dialog"][aria-modal="true"]')];
    const focusable = dialog => [...dialog.querySelectorAll(focusableSelector)].filter(element => element.getClientRects().length && !element.closest('[inert]'));

    new MutationObserver(() => {
        const dialogs = getDialogs();
        for (const dialog of previousDialogs) {
            if (!dialogs.includes(dialog)) {
                const opener = openers.get(dialog);
                openers.delete(dialog);
                if (opener?.isConnected) opener.focus();
            }
        }
        if (dialogs.length && !previousDialogs.length) {
            previousOverflow = document.body.style.overflow;
            document.body.style.overflow = 'hidden';
        }
        if (!dialogs.length && previousDialogs.length) document.body.style.overflow = previousOverflow;
        for (const dialog of dialogs) {
            if (!previousDialogs.includes(dialog)) {
                openers.set(dialog, document.activeElement);
                requestAnimationFrame(() => {
                    if (!dialog.isConnected) return;
                    dialog.tabIndex = -1;
                    (focusable(dialog)[0] ?? dialog).focus();
                });
            }
        }
        previousDialogs = dialogs;
    }).observe(document.body, { childList: true, subtree: true });

    document.addEventListener('keydown', event => {
        const dialog = getDialogs().at(-1);
        if (!dialog) return;
        if (event.key === 'Escape') {
            const close = dialog.querySelector('button.btn-close');
            if (close) { event.preventDefault(); close.click(); }
        }
        if (event.key !== 'Tab') return;
        const elements = focusable(dialog);
        const first = elements[0];
        const last = elements.at(-1);
        if (!first) { event.preventDefault(); dialog.focus(); return; }
        if (!dialog.contains(document.activeElement) || (event.shiftKey && document.activeElement === first)) {
            event.preventDefault(); (event.shiftKey ? last : first).focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault(); first.focus();
        }
    });
})();

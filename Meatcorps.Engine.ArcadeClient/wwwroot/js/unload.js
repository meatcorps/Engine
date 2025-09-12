export function register(dotNetRef) {
    const handler = () => { try { dotNetRef.invokeMethodAsync('OnPageUnloading'); } catch {} };
    document.addEventListener('visibilitychange', handler);
    window.addEventListener('beforeunload', handler);
    return { dispose: () => {
            document.removeEventListener('visibilitychange', handler);
            window.removeEventListener('beforeunload', handler);
        }};
}
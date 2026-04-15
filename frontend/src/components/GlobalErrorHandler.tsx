import { useEffect } from 'preact/hooks';
import { useNotifications } from './Notifications';

export function GlobalErrorHandler() {
    const { error } = useNotifications();

    useEffect(() => {
        let onError = (event: ErrorEvent) => {
            error(event.message || 'Global error');
        };

        let onUnhandledRejection = (event: PromiseRejectionEvent) => {
            error(event.reason?.message || 'Unhandled promise rejection');
        };

        window.addEventListener('error', onError);
        window.addEventListener('unhandledrejection', onUnhandledRejection);

        return () => {
            window.removeEventListener('error', onError);
            window.removeEventListener('unhandledrejection', onUnhandledRejection);
        };
    }, []);

    return null;
}

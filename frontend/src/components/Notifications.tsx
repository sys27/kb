import clsx from 'clsx';
import { ComponentChildren, createContext } from 'preact';
import { useContext, useState } from 'preact/hooks';

let id = 0;

type NotificationType = 'success' | 'error';

interface Notification {
    id: number;
    message: string;
    type: NotificationType;
}

interface NotificationContextValue {
    notify: (msg: string) => void;
    error: (msg: string) => void;
}

let NotificationContext = createContext<NotificationContextValue>({
    notify: () => {},
    error: () => {},
});

export const useNotifications = () => useContext(NotificationContext);

export function NotificationProvider({ children }: { children: ComponentChildren }) {
    let [notifications, setNotifications] = useState<Notification[]>([]);

    let addNotification = (message: string, type: NotificationType = 'success') => {
        let newNotification: Notification = {
            id: id++,
            message,
            type,
        };

        setNotifications(prev => [...prev, newNotification]);

        setTimeout(() => {
            setNotifications(prev => prev.filter(n => n.id !== newNotification.id));
        }, 3000);
    };

    let value = {
        notify: (msg: string) => addNotification(msg, 'success'),
        error: (msg: string) => addNotification(msg, 'error'),
    };

    return (
        <NotificationContext.Provider value={value}>
            {children}
            <NotificationContainer notifications={notifications} />
        </NotificationContext.Provider>
    );
}

function NotificationContainer({ notifications }: { notifications: Notification[] }) {
    return (
        <div className="fixed top-5 right-5 z-50 flex flex-col gap-3">
            {notifications.map(n => (
                <div
                    key={n.id}
                    class={clsx('min-w-55 max-w-sm px-4 py-3 rounded-lg shadow-xl/30 text-white opacity-90', {
                        'bg-red-700': n.type === 'error',
                        'bg-green-700': n.type === 'success',
                    })}>
                    <span>{n.message}</span>
                </div>
            ))}
        </div>
    );
}

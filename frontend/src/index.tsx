import { render } from 'preact';
import { ErrorBoundary, LocationProvider, Route, Router } from 'preact-iso';

import { GlobalErrorHandler } from './components/GlobalErrorHandler.js';
import { NotificationProvider, useNotifications } from './components/Notifications.js';
import { Home } from './pages/Home.js';
import { NotFound } from './pages/NotFound.js';
import './style.css';

export function App() {
    let { error } = useNotifications();

    return (
        <NotificationProvider>
            <GlobalErrorHandler />
            <ErrorBoundary onError={e => error(`An unexpected error occurred. ${e.message}`)}>
                <LocationProvider>
                    <Router>
                        <Route
                            path="/"
                            component={Home}
                        />
                        <Route
                            default
                            component={NotFound}
                        />
                    </Router>
                </LocationProvider>
            </ErrorBoundary>
        </NotificationProvider>
    );
}

render(<App />, document.getElementById('app')!);

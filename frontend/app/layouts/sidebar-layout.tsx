import { Outlet } from 'react-router';
import { SidebarComponent } from '~/components/sidebar-component';
import { SidebarProvider } from '~/components/ui/sidebar';

export default function SidebarLayout() {
    return (
        <SidebarProvider>
            <SidebarComponent />
            <main className="w-full">
                <Outlet />
            </main>
        </SidebarProvider>
    );
}

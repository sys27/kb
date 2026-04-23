import { Outlet } from 'react-router';
import { SidebarComponent } from '~/blocks/sidebar-component';
import { SidebarProvider } from '~/components/ui/sidebar';
import { getChats } from '~/services/chats';
import { getProjects } from '~/services/projects';
import type { Route } from './+types/sidebar-layout';

export async function clientLoader() {
    let projects = await getProjects();
    let chats = await getChats();

    return { projects, chats };
}

export default function SidebarLayout({ loaderData }: Route.ComponentProps) {
    let { projects, chats } = loaderData;

    return (
        <SidebarProvider>
            <SidebarComponent
                projects={projects}
                chats={chats}
            />
            <main className="w-full">
                <Outlet />
            </main>
        </SidebarProvider>
    );
}

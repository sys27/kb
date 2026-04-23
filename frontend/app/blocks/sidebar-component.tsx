import { Brain, Settings } from 'lucide-react';
import { Collapsible } from '~/components/ui/collapsible';
import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarHeader,
    SidebarMenu,
    SidebarMenuButton,
    SidebarMenuItem,
} from '~/components/ui/sidebar';
import type { Chat } from '~/services/chats';
import type { Project } from '~/services/projects';
import ChatsMenuList from './chats-menu-list';
import ProjectsMenuList from './projects-menu-list';

interface SidebarComponentProps {
    projects: Project[];
    chats: Chat[];
}

export function SidebarComponent({ projects, chats }: SidebarComponentProps) {
    return (
        <Sidebar
            variant="floating"
            // TODO: collapsible="none"
        >
            <SidebarHeader>
                <div className="flex flex-row items-center justify-center gap-2">
                    <Brain />
                    KB
                </div>
            </SidebarHeader>
            <SidebarContent>
                <Collapsible
                    defaultOpen
                    className="group/collapsible">
                    <ProjectsMenuList
                        projects={projects}
                        chats={chats}
                    />
                </Collapsible>
                <ChatsMenuList
                    projects={projects}
                    chats={chats}
                />
            </SidebarContent>
            <SidebarFooter>
                <SidebarMenu>
                    <SidebarMenuItem>
                        <SidebarMenuButton>
                            <Settings />
                            Settings
                        </SidebarMenuButton>
                    </SidebarMenuItem>
                </SidebarMenu>
            </SidebarFooter>
        </Sidebar>
    );
}

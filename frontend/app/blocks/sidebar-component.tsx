import { Brain, Moon, Settings, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import { useEffect, useState } from 'react';
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
    let { theme, setTheme } = useTheme();
    let [mounted, setMounted] = useState(false);

    useEffect(() => setMounted(true), []);

    if (!mounted) return null;

    return (
        <Sidebar
            variant="floating"
            // TODO: collapsible="none"
        >
            <SidebarHeader>
                <div className="flex flex-row items-center justify-center gap-2">
                    <Brain strokeWidth={1.5} />
                    KB
                </div>
            </SidebarHeader>
            <SidebarContent>
                <ProjectsMenuList
                    projects={projects}
                    chats={chats}
                />
                <ChatsMenuList
                    projects={projects}
                    chats={chats}
                />
            </SidebarContent>
            <SidebarFooter>
                <SidebarMenu>
                    <SidebarMenuItem onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}>
                        <SidebarMenuButton>
                            {theme === 'light' ? <Sun /> : <Moon />}
                            Theme
                        </SidebarMenuButton>
                    </SidebarMenuItem>
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

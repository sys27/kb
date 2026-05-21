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
import ChatsMenuList from './chats-menu-list';
import ProjectsMenuList from './projects-menu-list';

export function SidebarComponent() {
    let { theme, setTheme } = useTheme();
    let [mounted, setMounted] = useState(false);
    useEffect(() => setMounted(true), []);

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
                <ProjectsMenuList />
                <ChatsMenuList />
            </SidebarContent>
            <SidebarFooter>
                <SidebarMenu>
                    <SidebarMenuItem onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}>
                        <SidebarMenuButton>
                            {mounted ? theme === 'light' ? <Sun /> : <Moon /> : null}
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

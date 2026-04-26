import { useQuery } from '@tanstack/react-query';
import { Brain, Moon, Settings, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import { useEffect, useState } from 'react';
import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarGroup,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarHeader,
    SidebarMenu,
    SidebarMenuButton,
    SidebarMenuItem,
} from '~/components/ui/sidebar';
import { Skeleton } from '~/components/ui/skeleton';
import { chatsOptions } from '~/services/chats';
import { projectsOptions } from '~/services/projects';
import ChatsMenuList from './chats-menu-list';
import ProjectsMenuList from './projects-menu-list';

export function SidebarComponent() {
    let { theme, setTheme } = useTheme();
    let [mounted, setMounted] = useState(false);
    useEffect(() => setMounted(true), []);

    let { data: projects, isPending: isProjectsPending } = useQuery(projectsOptions);
    let { data: chats, isPending: isChatsPending } = useQuery(chatsOptions);

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
                {isProjectsPending || isChatsPending ? (
                    <>
                        <SidebarGroup>
                            <SidebarGroupLabel>Projects</SidebarGroupLabel>
                            <SidebarGroupContent>
                                <SidebarMenu>
                                    <Skeleton className="h-8 w-full" />
                                    <Skeleton className="h-8 w-full" />
                                    <Skeleton className="h-8 w-full" />
                                </SidebarMenu>
                            </SidebarGroupContent>
                        </SidebarGroup>
                        <SidebarGroup>
                            <SidebarGroupLabel>Chats</SidebarGroupLabel>
                            <SidebarGroupContent>
                                <SidebarMenu>
                                    <Skeleton className="h-8 w-full" />
                                    <Skeleton className="h-8 w-full" />
                                    <Skeleton className="h-8 w-full" />
                                </SidebarMenu>
                            </SidebarGroupContent>
                        </SidebarGroup>
                    </>
                ) : (
                    <>
                        <ProjectsMenuList
                            projects={projects!}
                            chats={chats!}
                        />
                        <ChatsMenuList
                            projects={projects!}
                            chats={chats!}
                        />
                    </>
                )}
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

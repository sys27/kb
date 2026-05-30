import { Brain, FolderPlus, MessageCirclePlus, Moon, Settings, Sun } from 'lucide-react';
import { useTheme } from 'next-themes';
import { useEffect, useState } from 'react';
import { Separator } from '~/components/ui/separator';
import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarGroup,
    SidebarGroupContent,
    SidebarHeader,
    SidebarMenu,
    SidebarMenuButton,
    SidebarMenuItem,
} from '~/components/ui/sidebar';
import ChatNewDialog from './chat-new-dialog';
import ChatsMenuList from './chats-menu-list';
import ProjectNewDialog from './project-new-dialog';
import ProjectsMenuList from './projects-menu-list';

export function SidebarComponent() {
    let { theme, setTheme } = useTheme();
    let [mounted, setMounted] = useState(false);
    useEffect(() => setMounted(true), []);

    let [openNewProjectDialog, setOpenNewProjectDialog] = useState(false);
    let [openNewChatDialog, setOpenNewChatDialog] = useState(false);

    return (
        <>
            <Sidebar variant="floating">
                <SidebarHeader>
                    <div className="flex flex-row items-center justify-center gap-2">
                        <Brain strokeWidth={1.5} />
                        KB
                    </div>
                </SidebarHeader>
                <SidebarContent className="flex-none">
                    <SidebarGroup>
                        <SidebarGroupContent>
                            <SidebarMenu>
                                <SidebarMenuItem>
                                    <SidebarMenuButton
                                        onClick={() => setOpenNewProjectDialog(true)}>
                                        <FolderPlus />
                                        New Project
                                    </SidebarMenuButton>
                                </SidebarMenuItem>
                                <SidebarMenuItem>
                                    <SidebarMenuButton onClick={() => setOpenNewChatDialog(true)}>
                                        <MessageCirclePlus />
                                        New Chat
                                    </SidebarMenuButton>
                                </SidebarMenuItem>
                            </SidebarMenu>
                        </SidebarGroupContent>
                    </SidebarGroup>
                </SidebarContent>
                <Separator />
                <SidebarContent>
                    <ProjectsMenuList />
                    <ChatsMenuList />
                </SidebarContent>
                <Separator />
                <SidebarFooter>
                    <SidebarMenu>
                        <SidebarMenuItem
                            onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}>
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

            <ProjectNewDialog
                open={openNewProjectDialog}
                onOpenChange={setOpenNewProjectDialog}
            />
            <ChatNewDialog
                open={openNewChatDialog}
                onOpenChange={setOpenNewChatDialog}
            />
        </>
    );
}

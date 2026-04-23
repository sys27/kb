import { Brain, FolderPlus, MessageCirclePlus, Settings } from 'lucide-react';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import {
    Sidebar,
    SidebarContent,
    SidebarFooter,
    SidebarGroup,
    SidebarGroupAction,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarHeader,
    SidebarMenu,
    SidebarMenuButton,
    SidebarMenuItem,
} from '~/components/ui/sidebar';
import ChatMenuItem from './chat-menu-item';
import ProjectMenuItem from './project-menu-item';

export function SidebarComponent({ projects, chats }: { projects: any[]; chats: any[] }) {
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
                    <SidebarGroup>
                        <SidebarGroupLabel asChild>
                            <CollapsibleTrigger>Projects</CollapsibleTrigger>
                        </SidebarGroupLabel>
                        <SidebarGroupAction>
                            <FolderPlus />
                        </SidebarGroupAction>
                        <CollapsibleContent>
                            <SidebarGroupContent>
                                <SidebarMenu>
                                    {projects.map(project => (
                                        <ProjectMenuItem
                                            key={project.id}
                                            project={project}
                                            chats={chats}
                                        />
                                    ))}
                                </SidebarMenu>
                            </SidebarGroupContent>
                        </CollapsibleContent>
                    </SidebarGroup>
                </Collapsible>
                <SidebarGroup>
                    <SidebarGroupLabel>Chats</SidebarGroupLabel>
                    <SidebarGroupAction>
                        <MessageCirclePlus />
                    </SidebarGroupAction>
                    <SidebarGroupContent>
                        <SidebarMenu>
                            {chats
                                .filter(x => x.projectId === null)
                                .map(chat => (
                                    <ChatMenuItem
                                        key={chat.id}
                                        chat={chat}
                                    />
                                ))}
                        </SidebarMenu>
                    </SidebarGroupContent>
                </SidebarGroup>
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

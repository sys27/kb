import { ChevronRight, MessageCirclePlus } from 'lucide-react';
import { useState } from 'react';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import {
    SidebarGroup,
    SidebarGroupAction,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarMenu,
} from '~/components/ui/sidebar';
import type { Chat } from '~/services/chats';
import type { Project } from '~/services/projects';
import ChatMenuItem from './chat-menu-item';
import ChatNewDialog from './chat-new-dialog';

interface ChatsMenuListProps {
    projects: Project[];
    chats: Chat[];
}

export default function ChatsMenuList({ projects, chats }: ChatsMenuListProps) {
    let [openNewDialog, setOpenNewDialog] = useState(false);

    return (
        <Collapsible
            defaultOpen
            className="group/collapsible">
            <SidebarGroup>
                <SidebarGroupLabel asChild>
                    <CollapsibleTrigger>
                        <ChevronRight className="transition-transform group-data-[state=open]/collapsible:rotate-90" />
                        Chats
                    </CollapsibleTrigger>
                </SidebarGroupLabel>
                <SidebarGroupAction>
                    <MessageCirclePlus onClick={() => setOpenNewDialog(true)} />

                    <ChatNewDialog
                        projects={projects}
                        open={openNewDialog}
                        onOpenChange={setOpenNewDialog}
                    />
                </SidebarGroupAction>
                <CollapsibleContent>
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
                </CollapsibleContent>
            </SidebarGroup>
        </Collapsible>
    );
}

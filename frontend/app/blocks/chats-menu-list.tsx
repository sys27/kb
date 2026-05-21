import { useQuery } from '@tanstack/react-query';
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
import { Skeleton } from '~/components/ui/skeleton';
import { chatsOptions } from '~/services/chats';
import ChatMenuItem from './chat-menu-item';
import ChatNewDialog from './chat-new-dialog';

export default function ChatsMenuList() {
    let { data: chats, isPending } = useQuery(chatsOptions);
    let [openNewDialog, setOpenNewDialog] = useState(false);

    return (
        <Collapsible
            defaultOpen
            className="group/collapsible">
            {isPending ? (
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
            ) : (
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
                            open={openNewDialog}
                            onOpenChange={setOpenNewDialog}
                        />
                    </SidebarGroupAction>
                    <CollapsibleContent>
                        <SidebarGroupContent>
                            <SidebarMenu>
                                {chats
                                    ?.filter(x => x.projectId === null)
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
            )}
        </Collapsible>
    );
}

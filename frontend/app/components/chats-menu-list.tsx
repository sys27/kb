import { useQuery } from '@tanstack/react-query';
import { ChevronRight } from 'lucide-react';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import {
    SidebarGroup,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarMenu,
} from '~/components/ui/sidebar';
import { Skeleton } from '~/components/ui/skeleton';
import { chatsOptions } from '~/services/chats';
import ChatMenuItem from './chat-menu-item';

export default function ChatsMenuList() {
    let { data: chats, isPending } = useQuery(chatsOptions);

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

import { Ellipsis, MessageCircle } from 'lucide-react';
import { Link } from 'react-router';
import { SidebarMenuAction, SidebarMenuButton, SidebarMenuItem } from '~/components/ui/sidebar';
import type { Chat } from '~/services/chats';

export default function ChatMenuItem({ chat }: { chat: Chat }) {
    return (
        <SidebarMenuItem
            key={chat.id}
            className="group/chat">
            <SidebarMenuButton asChild>
                <Link to={`/chats/${chat.id}`}>
                    <MessageCircle />
                    {chat.name}
                </Link>
            </SidebarMenuButton>
            <SidebarMenuAction className="opacity-0 transition-opacity group-hover/chat:opacity-100">
                <Ellipsis />
            </SidebarMenuAction>
        </SidebarMenuItem>
    );
}

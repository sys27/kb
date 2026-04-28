import { useQuery } from '@tanstack/react-query';
import { MessageCircle } from 'lucide-react';
import { Item, ItemContent, ItemDescription, ItemMedia, ItemTitle } from '~/components/ui/item';
import { projectChatsOptions } from '~/services/project-chats';

interface ProjectChatsListProps {
    projectId: number;
}

export default function ProjectChatsList({ projectId }: ProjectChatsListProps) {
    let { data: chats } = useQuery(projectChatsOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {chats && chats.length > 0 ? (
                chats.map(chat => (
                    <Item
                        key={chat.id}
                        variant="outline">
                        <ItemMedia variant="icon">
                            <MessageCircle />
                        </ItemMedia>
                        <ItemContent>
                            <ItemTitle>{chat.name}</ItemTitle>
                            <ItemDescription>{chat.lastMessage}</ItemDescription>
                        </ItemContent>
                        <ItemContent>
                            <ItemDescription>
                                {chat.lastMessageAt
                                    ? new Date(chat.lastMessageAt).toLocaleDateString()
                                    : null}
                            </ItemDescription>
                        </ItemContent>
                    </Item>
                ))
            ) : (
                <div className="p-4 text-center text-sm text-muted-foreground">No chats found.</div>
            )}
        </div>
    );
}

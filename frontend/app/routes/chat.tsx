import { useQuery } from '@tanstack/react-query';
import { FileText, MessageCircle, Plus, Search, Send } from 'lucide-react';
import MessageItem from '~/blocks/message-item';
import MessageSkeletonItem from '~/blocks/message-skeleton-item';
import {
    DropdownMenu,
    DropdownMenuCheckboxItem,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from '~/components/ui/dropdown-menu';
import {
    Empty,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from '~/components/ui/empty';
import {
    InputGroup,
    InputGroupAddon,
    InputGroupButton,
    InputGroupTextarea,
} from '~/components/ui/input-group';
import { ScrollArea } from '~/components/ui/scroll-area';
import { messagesOptions } from '~/services/messages';
import type { Route } from './+types/chat';

export default function Chat({ params }: Route.ComponentProps) {
    let chatId = Number(params.chatId);
    let { data: messages, isPending } = useQuery(messagesOptions(chatId));

    return (
        <div className="flex h-screen w-full flex-col gap-4 p-2">
            <ScrollArea className="flex-1 overflow-y-auto">
                <div className="flex flex-col gap-2 p-1">
                    {isPending ? (
                        <>
                            <MessageSkeletonItem />
                            <MessageSkeletonItem />
                            <MessageSkeletonItem />
                        </>
                    ) : messages && messages.length > 0 ? (
                        messages.map(message => (
                            <MessageItem
                                key={message.id}
                                message={message}
                            />
                        ))
                    ) : (
                        <Empty>
                            <EmptyHeader>
                                <EmptyMedia variant="icon">
                                    <MessageCircle />
                                </EmptyMedia>
                                <EmptyTitle>No Messages Yet</EmptyTitle>
                                <EmptyDescription>
                                    You haven&apos;t sent any messages yet. Get started by sending
                                    your first message.
                                </EmptyDescription>
                            </EmptyHeader>
                        </Empty>
                    )}
                </div>
            </ScrollArea>
            <InputGroup className="flex-none">
                <InputGroupTextarea
                    id="message"
                    placeholder="Ask anything..."
                    className="max-h-20 min-h-20"
                />
                <InputGroupAddon
                    align="block-end"
                    className="flex flex-row justify-between">
                    <InputGroupButton>
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <Plus />
                            </DropdownMenuTrigger>
                            <DropdownMenuContent className="w-48">
                                <DropdownMenuItem disabled>
                                    <FileText />
                                    Add File
                                </DropdownMenuItem>
                                <DropdownMenuCheckboxItem checked={true}>
                                    <Search />
                                    Web Search
                                </DropdownMenuCheckboxItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </InputGroupButton>
                    <InputGroupButton>
                        <Send />
                    </InputGroupButton>
                </InputGroupAddon>
            </InputGroup>
        </div>
    );
}

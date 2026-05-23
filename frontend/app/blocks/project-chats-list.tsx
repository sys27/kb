import { useQuery } from '@tanstack/react-query';
import { ChevronsUpDown } from 'lucide-react';
import { Link } from 'react-router';
import { Button } from '~/components/ui/button';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
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
                    <Collapsible
                        key={chat.id}
                        defaultOpen
                        className="group rounded-xl border bg-card transition-colors hover:bg-accent/20">
                        <div className="flex items-center justify-between p-4">
                            <div className="min-w-0">
                                <Link
                                    to={`/chats/${chat.id}`}
                                    className="truncate text-base font-semibold hover:underline">
                                    {chat.name}
                                </Link>

                                <p className="text-sm text-muted-foreground">
                                    {chat.lastMessageAt
                                        ? chat.lastMessageAt.toLocaleString()
                                        : null}
                                </p>
                            </div>

                            <CollapsibleTrigger asChild>
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    className="shrink-0">
                                    <ChevronsUpDown className="size-4" />
                                </Button>
                            </CollapsibleTrigger>
                        </div>

                        <CollapsibleContent>
                            <div className="border-t px-4 py-3">
                                <div className="flex flex-col gap-3">
                                    <div className="rounded-lg border bg-background p-3">
                                        <div className="flex items-start gap-3">
                                            <div className="min-w-0 flex-1">
                                                <p className="line-clamp-3 text-sm leading-relaxed text-muted-foreground">
                                                    {chat.lastMessage}
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </CollapsibleContent>
                    </Collapsible>
                ))
            ) : (
                <div className="p-4 text-center text-sm text-muted-foreground">No chats found.</div>
            )}
        </div>
    );
}

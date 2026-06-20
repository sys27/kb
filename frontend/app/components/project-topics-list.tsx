import { useQuery } from '@tanstack/react-query';
import { ChevronsUpDown, List } from 'lucide-react';
import { Link } from 'react-router';
import { Button } from '~/components/ui/button';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import {
    Empty,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from '~/components/ui/empty';
import { projectTopicsOptions } from '~/services/project-topics';

interface ProjectTopicsListProps {
    projectId: number;
}

export function ProjectTopicsList({ projectId }: ProjectTopicsListProps) {
    let { data: topics } = useQuery(projectTopicsOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {topics && topics.length > 0 ? (
                topics.map(chat => (
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
                                    {chat.topics.length} topics
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
                                    {chat.topics.map(topic => (
                                        <div className="rounded-lg border bg-background p-3">
                                            <div className="flex items-start gap-3">
                                                <div className="min-w-0 flex-1">
                                                    <p className="text-sm leading-relaxed text-muted-foreground">
                                                        {topic.name}
                                                    </p>
                                                </div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </CollapsibleContent>
                    </Collapsible>
                ))
            ) : (
                <Empty>
                    <EmptyHeader>
                        <EmptyMedia variant="icon">
                            <List />
                        </EmptyMedia>
                        <EmptyTitle>No Topics Yet</EmptyTitle>
                        <EmptyDescription>
                            No topics have been gathered for this project.
                        </EmptyDescription>
                    </EmptyHeader>
                </Empty>
            )}
        </div>
    );
}

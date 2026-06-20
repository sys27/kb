import { useQuery } from '@tanstack/react-query';
import { ChevronsUpDown, NotebookText } from 'lucide-react';
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
import { projectFactsOptions } from '~/services/project-facts';

interface ProjectFactsListProps {
    projectId: number;
}

export function ProjectFactsList({ projectId }: ProjectFactsListProps) {
    let { data: facts } = useQuery(projectFactsOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {facts && facts.length > 0 ? (
                facts.map(chat => (
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
                                    {chat.facts.length} facts
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
                                    {chat.facts.map(fact => (
                                        <div className="rounded-lg border bg-background p-3">
                                            <div className="flex items-start gap-3">
                                                <div className="min-w-0 flex-1">
                                                    <p className="text-sm leading-relaxed text-muted-foreground">
                                                        {fact.name}
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
                            <NotebookText />
                        </EmptyMedia>
                        <EmptyTitle>No Facts Yet</EmptyTitle>
                        <EmptyDescription>
                            No facts have been recorded for this project.
                        </EmptyDescription>
                    </EmptyHeader>
                </Empty>
            )}
        </div>
    );
}

import { useQuery } from '@tanstack/react-query';
import { CheckCircle2, ChevronsUpDown, UserCheck } from 'lucide-react';
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
import { projectDecisionsOptions } from '~/services/project-decisions';

interface ProjectDecisionsListProps {
    projectId: number;
}

export function ProjectDecisionsList({ projectId }: ProjectDecisionsListProps) {
    let { data: decisions } = useQuery(projectDecisionsOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {decisions && decisions.length > 0 ? (
                decisions.map(chat => (
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
                                    {chat.decisions.length} decisions
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
                                    {chat.decisions.map(decision => (
                                        <div className="rounded-lg border bg-background p-3">
                                            <div className="flex items-start gap-3">
                                                <div className="mt-0.5 text-primary">
                                                    <CheckCircle2 className="size-4" />
                                                </div>

                                                <div className="min-w-0 flex-1">
                                                    <h4 className="leading-none font-medium">
                                                        {decision.decision}
                                                    </h4>

                                                    <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
                                                        {decision.reason}
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
                            <UserCheck />
                        </EmptyMedia>
                        <EmptyTitle>No Decisions Yet</EmptyTitle>
                        <EmptyDescription>
                            No decisions have been recorded for this project.
                        </EmptyDescription>
                    </EmptyHeader>
                </Empty>
            )}
        </div>
    );
}

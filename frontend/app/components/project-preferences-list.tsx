import { useQuery } from '@tanstack/react-query';
import { ChevronsUpDown, UserStar } from 'lucide-react';
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
import { projectPreferencesOptions } from '~/services/project-preferences';

interface ProjectPreferencesListProps {
    projectId: number;
}

export function ProjectPreferencesList({ projectId }: ProjectPreferencesListProps) {
    let { data: preferences } = useQuery(projectPreferencesOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {preferences && preferences.length > 0 ? (
                preferences.map(chat => (
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
                                    {chat.userPreferences.length} preferences
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
                                    {chat.userPreferences.map(preference => (
                                        <div className="rounded-lg border bg-background p-3">
                                            <div className="flex items-start gap-3">
                                                <div className="min-w-0 flex-1">
                                                    <p className="text-sm leading-relaxed text-muted-foreground">
                                                        {preference.name}
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
                            <UserStar />
                        </EmptyMedia>
                        <EmptyTitle>No Preferences Yet</EmptyTitle>
                        <EmptyDescription>
                            No user preferences have been gathered for this project.
                        </EmptyDescription>
                    </EmptyHeader>
                </Empty>
            )}
        </div>
    );
}

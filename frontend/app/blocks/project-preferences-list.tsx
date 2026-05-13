import { useQuery } from '@tanstack/react-query';
import { UserStar } from 'lucide-react';
import { Link } from 'react-router';
import {
    Empty,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from '~/components/ui/empty';
import { Item, ItemContent, ItemTitle } from '~/components/ui/item';
import { projectPreferencesOptions } from '~/services/project-preferences';

interface ProjectPreferencesListProps {
    projectId: number;
}

export function ProjectPreferencesList({ projectId }: ProjectPreferencesListProps) {
    let { data: preferences } = useQuery(projectPreferencesOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {preferences && preferences.length > 0 ? (
                preferences.map(preference => (
                    <Item
                        key={preference.id}
                        variant="outline">
                        <ItemContent>
                            <ItemTitle>{preference.preference}</ItemTitle>
                        </ItemContent>
                        <ItemContent>
                            <Link to={`/chats/${preference.chat.id}`}>{preference.chat.name}</Link>
                        </ItemContent>
                    </Item>
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

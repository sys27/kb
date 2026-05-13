import { useQuery } from '@tanstack/react-query';
import { UserCheck } from 'lucide-react';
import { Link } from 'react-router';
import {
    Empty,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from '~/components/ui/empty';
import { Item, ItemContent, ItemDescription, ItemTitle } from '~/components/ui/item';
import { projectDecisionsOptions } from '~/services/project-decisions';

interface ProjectDecisionsListProps {
    projectId: number;
}

export function ProjectDecisionsList({ projectId }: ProjectDecisionsListProps) {
    let { data: decisions } = useQuery(projectDecisionsOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {decisions && decisions.length > 0 ? (
                decisions.map(decision => (
                    <Item
                        key={decision.id}
                        variant="outline">
                        <ItemContent>
                            <ItemTitle>{decision.decision}</ItemTitle>
                            <ItemDescription>{decision.reason}</ItemDescription>
                        </ItemContent>
                        <ItemContent>
                            <ItemDescription>
                                <Link to={`/chats/${decision.chat.id}`}>{decision.chat.name}</Link>
                            </ItemDescription>
                        </ItemContent>
                    </Item>
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

import { useQuery } from '@tanstack/react-query';
import { NotebookText } from 'lucide-react';
import { Link } from 'react-router';
import {
    Empty,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from '~/components/ui/empty';
import { Item, ItemContent, ItemTitle } from '~/components/ui/item';
import { projectFactsOptions } from '~/services/project-facts';

interface ProjectFactsListProps {
    projectId: number;
}

export function ProjectFactsList({ projectId }: ProjectFactsListProps) {
    let { data: facts } = useQuery(projectFactsOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {facts && facts.length > 0 ? (
                facts.map(fact => (
                    <Item
                        key={fact.id}
                        variant="outline">
                        <ItemContent>
                            <ItemTitle>{fact.fact}</ItemTitle>
                        </ItemContent>
                        <ItemContent>
                            <Link to={`/chats/${fact.chat.id}`}>{fact.chat.name}</Link>
                        </ItemContent>
                    </Item>
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

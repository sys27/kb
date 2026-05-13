import { useQuery } from '@tanstack/react-query';
import { List } from 'lucide-react';
import { Link } from 'react-router';
import {
    Empty,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from '~/components/ui/empty';
import { Item, ItemContent, ItemTitle } from '~/components/ui/item';
import { projectTopicsOptions } from '~/services/project-topics';

interface ProjectTopicsListProps {
    projectId: number;
}

export function ProjectTopicsList({ projectId }: ProjectTopicsListProps) {
    let { data: topics } = useQuery(projectTopicsOptions(projectId));

    return (
        <div className="flex flex-col gap-2 p-2">
            {topics && topics.length > 0 ? (
                topics.map(topic => (
                    <Item
                        key={topic.id}
                        variant="outline">
                        <ItemContent>
                            <ItemTitle>{topic.topic}</ItemTitle>
                        </ItemContent>
                        <ItemContent>
                            <Link to={`/chats/${topic.chat.id}`}>{topic.chat.name}</Link>
                        </ItemContent>
                    </Item>
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

import { Brain } from 'lucide-react';
import { Empty, EmptyHeader, EmptyMedia, EmptyTitle } from '~/components/ui/empty';

export function meta() {
    return [{ title: 'KB' }, { name: 'description', content: 'Personal Knowledge Base' }];
}

export default function Home() {
    return (
        <Empty className="min-h-screen">
            <EmptyHeader>
                <EmptyMedia className="size-16">
                    <Brain className="size-8" />
                </EmptyMedia>
                <EmptyTitle className="text-lg">Knowledge Base</EmptyTitle>
            </EmptyHeader>
        </Empty>
    );
}

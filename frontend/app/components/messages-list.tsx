import { MessageCircle } from 'lucide-react';
import {
    Empty,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from '~/components/ui/empty';
import { MessageType, type Message } from '~/services/messages';
import MessageItem from './message-item';
import MessageSkeletonItem from './message-skeleton-item';

export function MessagesList({
    messages,
    isPending,
}: {
    messages?: Message[];
    isPending: boolean;
}) {
    if (isPending) {
        return (
            <>
                <MessageSkeletonItem />
                <MessageSkeletonItem />
                <MessageSkeletonItem />
            </>
        );
    }

    if (messages && messages.length > 0) {
        let toolResults: Record<string, Message> = {};
        for (let m of messages) {
            if (m.messageTypeId === MessageType.toolResultId) {
                let data: { callId: string };
                try {
                    data = JSON.parse(m.text) as { callId: string };
                } catch {
                    continue;
                }

                toolResults[data.callId] = m;
            }
        }

        return messages.map(message => (
            <MessageItem
                key={message.id}
                message={message}
                toolResults={toolResults}
            />
        ));
    }

    return (
        <Empty>
            <EmptyHeader>
                <EmptyMedia variant="icon">
                    <MessageCircle />
                </EmptyMedia>
                <EmptyTitle>No Messages Yet</EmptyTitle>
                <EmptyDescription>
                    You haven&apos;t sent any messages yet. Get started by sending your first
                    message.
                </EmptyDescription>
            </EmptyHeader>
        </Empty>
    );
}

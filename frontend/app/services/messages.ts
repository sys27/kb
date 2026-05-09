import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const MessageSchema = z.object({
    id: z.number(),
    role: z.enum(['System', 'User', 'Assistant', 'Tool']),
    kind: z.enum(['Text', 'Reasoning']),
    text: z.string(),
    timestamp: z.coerce.date(),
});

const MessageSchemaSse = z.object({
    role: z.enum(['System', 'User', 'Assistant', 'Tool']),
    kind: z.enum(['Text', 'Reasoning']),
    text: z.string(),
});

export type Message = z.infer<typeof MessageSchema>;

export type MessageSse = z.infer<typeof MessageSchemaSse>;

export function messagesOptions(chatId: number) {
    return queryOptions({
        queryKey: ['messages', chatId],
        queryFn: () => getMessages(chatId),
        staleTime: Infinity,
    });
}

export async function getMessages(chatId: number): Promise<Message[]> {
    let response = await fetch(`/api/chats/${chatId}/messages`);
    if (!response.ok) {
        throw new Error('Failed to fetch messages');
    }

    let json = await response.json();

    return z
        .array(MessageSchema)
        .parse(json)
        .filter(x => x.role != 'System');
}

export async function* sendMessage(chatId: number, message: string) {
    let request = {
        text: message,
    };
    let response = await fetch(`/api/chats/${chatId}/messages`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        throw new Error('Failed to send message');
    }

    if (!response.body) {
        throw new Error('The body is empty');
    }

    let reader = response.body.getReader();
    let decoder = new TextDecoder();

    let buffer = '';

    try {
        const dataPrefix = 'data: ';

        while (true) {
            let { done, value } = await reader.read();
            if (done) {
                break;
            }

            buffer += decoder.decode(value, { stream: true });
            let chunks = buffer.split('\n\n');
            buffer = chunks.pop() || '';

            for (let part of chunks) {
                if (!part.startsWith(dataPrefix)) {
                    continue;
                }

                let event: MessageSse = JSON.parse(part.substring(dataPrefix.length));

                yield MessageSchemaSse.parse(event);
            }
        }

        buffer += decoder.decode();

        if (buffer.startsWith('data: ')) {
            let event = JSON.parse(buffer.substring(dataPrefix.length));

            yield MessageSchemaSse.parse(event);
        }
    } finally {
        reader.releaseLock();
    }
}

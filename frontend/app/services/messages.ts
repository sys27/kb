import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

export const MessageType = {
    systemId: 1,
    assistantReasoningId: 2,
    assistantAnswerId: 3,
    userContextId: 4,
    userRequestId: 5,
    toolCallId: 6,
    toolResultId: 7,
} as const;

const MessageTypeRoles: Record<(typeof MessageType)[keyof typeof MessageType], string> = {
    [MessageType.systemId]: 'System',
    [MessageType.assistantReasoningId]: 'Assistant',
    [MessageType.assistantAnswerId]: 'Assistant',
    [MessageType.userContextId]: 'User',
    [MessageType.userRequestId]: 'User',
    [MessageType.toolCallId]: 'Tool',
    [MessageType.toolResultId]: 'Tool',
};

const MessageTypeKinds: Record<(typeof MessageType)[keyof typeof MessageType], string> = {
    [MessageType.systemId]: '',
    [MessageType.assistantReasoningId]: 'Reasoning',
    [MessageType.assistantAnswerId]: '',
    [MessageType.userContextId]: 'Context',
    [MessageType.userRequestId]: '',
    [MessageType.toolCallId]: '',
    [MessageType.toolResultId]: '',
};

const MessageSchema = z.object({
    id: z.number(),
    messageTypeId: z.enum(MessageType),
    text: z.string(),
    timestamp: z.coerce.date(),
});

const MessageSchemaSse = z.object({
    messageTypeId: z.enum(MessageType),
    text: z.string(),
});

export type Message = z.infer<typeof MessageSchema>;

export type MessageSse = z.infer<typeof MessageSchemaSse>;

export function messagesOptions(chatId: number) {
    return queryOptions({
        queryKey: ['chats', chatId, 'messages'],
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

    // TODO: server side filtering
    return z
        .array(MessageSchema)
        .parse(json)
        .filter(x => x.messageTypeId != MessageType.systemId);
}

export async function* sendMessage(chatId: number, message: string, enableWebSearch: boolean) {
    let request = {
        text: message,
        enableWebSearch: enableWebSearch,
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

export function getMessageRoleName(message: Message): string {
    return MessageTypeRoles[message.messageTypeId];
}

export function getMessageKindName(message: Message): string {
    return MessageTypeKinds[message.messageTypeId];
}

import { queryOptions } from "@tanstack/react-query";

export interface Message {
    get id(): number;
    get role(): 'System' | 'User' | 'Assistant' | 'Tool';
    get kind(): 'Text' | 'Reasoning';
    get text(): string;
    get timestamp(): Date;
}

export function messagesOptions(chatId: number) {
    return queryOptions({
        queryKey: ['messages', chatId],
        queryFn: () => getMessages(chatId),
    });
}

export async function getMessages(chatId: number): Promise<Message[]> {
    let response = await fetch(`/api/chats/${chatId}/messages`);
    if (!response.ok) {
        throw new Error('Failed to fetch messages');
    }

    return await response.json();
}
import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const ChatSchema = z.object({
    id: z.number(),
    name: z.string(),
    projectId: z.number().nullable(),
});

export type Chat = z.infer<typeof ChatSchema>;

export const chatsOptions = queryOptions({
    queryKey: ['chats'],
    queryFn: getChats,
    staleTime: Infinity,
});

export async function getChats(): Promise<Chat[]> {
    let response = await fetch('/api/chats');
    if (!response.ok) {
        throw new Error('Failed to fetch chats');
    }

    let json = await response.json();
    return z.array(ChatSchema).parse(json);
}

export async function createChat(name: string, projectId: number | null): Promise<Chat> {
    let response = await fetch('/api/chats', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ name, projectId }),
    });
    if (!response.ok) {
        throw new Error('Failed to create chat');
    }

    let json = await response.json();

    return ChatSchema.parse(json);
}

export async function updateChat(
    id: number,
    name: string,
    projectId: number | null,
): Promise<Chat> {
    let response = await fetch(`/api/chats/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ name, projectId }),
    });
    if (!response.ok) {
        throw new Error('Failed to update chat');
    }

    let json = await response.json();
    return ChatSchema.parse(json);
}

export async function deleteChat(id: number) {
    let response = await fetch(`/api/chats/${id}`, {
        method: 'DELETE',
    });
    if (!response.ok) {
        throw new Error('Failed to delete chat');
    }
}

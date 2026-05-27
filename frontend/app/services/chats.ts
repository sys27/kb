import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const ChatSchema = z.object({
    id: z.number(),
    name: z.string(),
    projectId: z.number().nullable(),
});

export type Chat = z.infer<typeof ChatSchema>;

const FollowUpQuestions = z.object({
    questions: z.array(z.string()),
});

export type FollowUpQuestions = z.infer<typeof FollowUpQuestions>;

export const chatsOptions = queryOptions({
    queryKey: ['chats'],
    queryFn: getChats,
    staleTime: Infinity,
});

export function chatOptions(id: number) {
    return queryOptions({
        queryKey: ['chats', id],
        staleTime: Infinity,
    });
}

export function followUpQuestionsOptions(id: number) {
    return queryOptions({
        queryKey: ['chats', id, 'follow-ups'],
        queryFn: () => getFollowUpQuestions(id),
        staleTime: Infinity,
    });
}

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

export async function generateName(id: number): Promise<string> {
    let response = await fetch(`/api/chats/${id}/generate-name`, {
        method: 'POST',
    });
    if (!response.ok) {
        throw new Error('Failed to generate chat name');
    }

    let json = await response.json();
    return json.name;
}

export async function getFollowUpQuestions(id: number): Promise<FollowUpQuestions> {
    let response = await fetch(`/api/chats/${id}/follow-ups`, {
        method: 'POST',
    });
    if (!response.ok) {
        throw new Error('Failed to fetch follow-up questions');
    }

    let json = await response.json();
    return FollowUpQuestions.parse(json);
}

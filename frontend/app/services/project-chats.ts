import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const ProjectChatSchema = z.object({
    id: z.number(),
    name: z.string(),
    lastMessage: z.string().nullable(),
    lastMessageAt: z.coerce.date().nullable(),
});

export type ProjectChat = z.infer<typeof ProjectChatSchema>;

export function projectChatsOptions(projectId: number) {
    return queryOptions({
        queryKey: ['project', projectId, 'chats'],
        queryFn: () => getProjectChats(projectId),
        staleTime: Infinity,
    });
}

export async function getProjectChats(projectId: number): Promise<ProjectChat[]> {
    let response = await fetch(`/api/projects/${projectId}/chats`);
    if (!response.ok) {
        throw new Error('Failed to fetch project chats');
    }

    let json = await response.json();

    return z.array(ProjectChatSchema).parse(json);
}

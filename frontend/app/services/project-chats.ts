import { queryOptions } from "@tanstack/react-query";

export interface ProjectChat {
    id: number;
    name: string;
    lastMessage: string | null;
    lastMessageAt: string | null;
}

export function projectChatsOptions(projectId: number) {
    return queryOptions({
        queryKey: ["project", projectId, "chats"],
        queryFn: () => getProjectChats(projectId),
    });
}

export async function getProjectChats(projectId: number): Promise<ProjectChat[]> {
    let response = await fetch(`/api/projects/${projectId}/chats`);
    if (!response.ok) {
        throw new Error("Failed to fetch project chats");
    }

    return response.json();
}
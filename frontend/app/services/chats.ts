import { queryOptions } from "@tanstack/react-query";

export interface Chat {
    get id(): number;
    get name(): string;
    get projectId(): number | null;
}

export const chatsOptions = queryOptions({
    queryKey: ["chats"],
    queryFn: getChats,
    staleTime: Infinity,
});

export async function getChats(): Promise<Chat[]> {
    let response = await fetch("/api/chats");
    if (!response.ok) {
        throw new Error("Failed to fetch chats");
    }

    return response.json();
}

export async function createChat(name: string, projectId?: number) {
    let response = await fetch("/api/chats", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ name, projectId }),
    });
    if (!response.ok) {
        throw new Error("Failed to create chat");
    }

    return response.json();
}

export async function updateChat(id: number, name: string) {
    let response = await fetch(`/api/chats/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ name }),
    });
    if (!response.ok) {
        throw new Error("Failed to update chat");
    }

    return response.json();
}

export async function deleteChat(id: number) {
    let response = await fetch(`/api/chats/${id}`, {
        method: "DELETE",
    });
    if (!response.ok) {
        throw new Error("Failed to delete chat");
    }
}
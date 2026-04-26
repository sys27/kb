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
import { ChatListResponse } from "../models/ChatListResponse";

export class ChatApis {
    public static async getChats(): Promise<ChatListResponse[]> {
        let response = await fetch('/api/chats');
        if (!response.ok) {
            throw new Error('Failed to fetch chats');
        }

        return response.json();
    }

    public static async getChat(chatId: number): Promise<ChatListResponse> {
        let response = await fetch(`/api/chats/${chatId}`);
        if (!response.ok) {
            throw new Error('Failed to fetch chat');
        }

        return response.json();
    }

    public static async createChat(title: string): Promise<ChatListResponse> {
        let response = await fetch('/api/chats', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ title })
        });
        if (!response.ok) {
            throw new Error('Failed to create chat');
        }

        return response.json();
    }

    public static async renameChat(chatId: number, newTitle: string): Promise<ChatListResponse> {
        let response = await fetch(`/api/chats/${chatId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ title: newTitle })
        });
        if (!response.ok) {
            throw new Error('Failed to rename chat');
        }

        return response.json();
    }

    public static async deleteChat(chatId: number): Promise<void> {
        let response = await fetch(`/api/chats/${chatId}`, { method: 'DELETE' });
        if (!response.ok) {
            throw new Error('Failed to delete chat');
        }
    }
}
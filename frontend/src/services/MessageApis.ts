import { MessageListResponse } from "../models/MessageListResponse";

export class MessageApis {
    public static async getMessages(chatId: number): Promise<MessageListResponse[]> {
        let response = await fetch(`/api/chats/${chatId}/messages`);
        if (!response.ok) {
            throw new Error('Failed to fetch messages');
        }

        return response.json();
    }
}
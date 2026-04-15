import { MessageCirclePlus } from 'lucide-preact';
import { useState } from 'preact/hooks';
import { ChatListResponse } from '../models/ChatListResponse';
import { Chat } from './Chat';

export function ChatList() {
    let [chats, setChats] = useState<ChatListResponse[]>([
        { id: 1, name: 'Chat 1' },
        { id: 2, name: 'Chat 2' },
        { id: 3, name: 'Chat 3' },
    ]);

    return (
        <>
            <div class="flex group">
                <span class="flex-1 font-bold">Chats</span>
                <MessageCirclePlus
                    strokeWidth={1.5}
                    class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                />
            </div>
            {chats.map(chat => (
                <Chat
                    key={chat.id}
                    name={chat.name}
                />
            ))}
        </>
    );
}

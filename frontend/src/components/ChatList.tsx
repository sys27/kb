import { MessageCirclePlus } from 'lucide-preact';
import { useEffect, useState } from 'preact/hooks';
import { ChatListResponse } from '../models/ChatListResponse';
import { ChatApis } from '../services/ChatApis';
import { Chat } from './Chat';
import { useModal } from './Modal';

export function ChatList() {
    let [chats, setChats] = useState<ChatListResponse[]>([]);
    let { openModal, ModalRenderer: CreateChat } = useModal();

    useEffect(() => {
        ChatApis.getChats().then(setChats);
    }, []);

    async function createChat(e: Event): Promise<boolean> {
        let form = e.target as HTMLFormElement;
        if (!form.checkValidity()) {
            form.reportValidity();
            return false;
        }

        let formData = new FormData(form);
        let title = formData.get('title') as string;
        if (!title?.trim()) {
            return false;
        }

        let newChat = await ChatApis.createChat(title);
        setChats(prev => [newChat, ...prev]);

        return true;
    }

    return (
        <>
            <div class="flex group">
                <span class="flex-1 font-bold">Chats</span>
                <button
                    class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                    onClick={() =>
                        openModal({
                            onOk: createChat,
                        })
                    }>
                    <MessageCirclePlus strokeWidth={1.5} />
                </button>
            </div>
            {chats.map(chat => (
                <Chat
                    key={chat.id}
                    {...chat}
                />
            ))}

            <CreateChat title="Create Chat">
                <label>
                    Title:
                    <input
                        type="text"
                        name="title"
                        required
                        class="border border-neutral-300 rounded-lg px-2 py-1 w-full"
                    />
                </label>
            </CreateChat>
        </>
    );
}

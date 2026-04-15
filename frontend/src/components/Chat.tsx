import { Ellipsis, MessageCircle } from 'lucide-preact';
import { ChatListResponse } from '../models/ChatListResponse';

export function Chat({ title }: ChatListResponse) {
    return (
        <div class="flex gap-1 group pt-1 pl-4 pr-4 pb-1 hover:bg-neutral-200 rounded-lg">
            <MessageCircle strokeWidth={1.5} />
            <span class="flex-1">{title}</span>
            <Ellipsis
                strokeWidth={1.5}
                class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
            />
        </div>
    );
}

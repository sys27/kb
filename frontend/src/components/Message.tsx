import clsx from 'clsx';
import { Copy } from 'lucide-preact';
import { MessageListResponse } from '../models/MessageListResponse';

export function Message({ kind, text, timestamp }: MessageListResponse) {
    let userMessage = kind === 'user';

    return (
        <div class={clsx('flex flex-col gap-2 group', { 'w-xl': userMessage })}>
            <div class={clsx('space-y-2', { 'bg-neutral-200 p-4 rounded-xl': userMessage })}>
                <p>{text}</p>
            </div>
            <div class="flex flex-row gap-2">
                <Copy
                    size={20}
                    strokeWidth={1.5}
                />
                <span class="ml-auto opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                    {timestamp.toLocaleTimeString()}
                </span>
            </div>
        </div>
    );
}

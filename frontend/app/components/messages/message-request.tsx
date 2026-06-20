import { Copy } from 'lucide-react';
import { Button } from '~/components/ui/button';
import { parseMarkdown } from '~/services/md';
import type { Message } from '~/services/messages';

async function handleCopy(text: string) {
    await navigator.clipboard.writeText(text);
}

export function MessageRequest({ message }: { message: Message }) {
    let md = parseMarkdown(message.text);

    return (
        <div className="group flex max-w-md flex-col self-end">
            <div className="rounded-2xl border bg-muted px-4 py-2">
                <article
                    className="prose max-w-none dark:prose-invert"
                    dangerouslySetInnerHTML={{ __html: md }}
                />
            </div>
            <div className="flex flex-row justify-between px-2 py-0 opacity-0 transition-opacity group-hover:opacity-100">
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => handleCopy(message.text)}>
                    <Copy />
                </Button>
                <span className="text-muted-foreground">{message.timestamp.toLocaleString()}</span>
            </div>
        </div>
    );
}

import { Sparkles } from 'lucide-react';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import { parseMarkdown } from '~/services/md';
import type { Message } from '~/services/messages';

export function MessageReasoning({ message }: { message: Message }) {
    let md = parseMarkdown(message.text);

    return (
        <div className="rounded-2xl border py-2 pl-4">
            <Collapsible defaultOpen={false}>
                <CollapsibleTrigger asChild>
                    <div className="flex flex-row gap-2 text-sm text-muted-foreground">
                        <Sparkles
                            strokeWidth={1.5}
                            size={20}
                        />
                        Thinking
                    </div>
                </CollapsibleTrigger>
                <CollapsibleContent asChild>
                    <div className="p-4">
                        <article
                            className="prose max-w-none dark:prose-invert"
                            dangerouslySetInnerHTML={{ __html: md }}
                        />
                    </div>
                </CollapsibleContent>
            </Collapsible>
        </div>
    );
}

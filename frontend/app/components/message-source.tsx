import { FileText, Globe } from 'lucide-react';
import { Tooltip, TooltipContent, TooltipTrigger } from '~/components/ui/tooltip';
import type { Message } from '~/services/messages';

interface MessageSource {
    sourceType: 'Document' | 'WebSite';
    source: string;
}

export function MessageSource({ message }: { message: Message }) {
    let messageSource = JSON.parse(message.text) as MessageSource;

    return (
        <div className="flex max-w-md flex-row gap-2 self-end rounded-2xl border bg-muted px-4 py-2">
            {messageSource.sourceType === 'WebSite' ? (
                <a
                    href={messageSource.source}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-1.5 truncate hover:underline">
                    <Globe
                        className="inline"
                        strokeWidth={1.5}
                        size={20}
                    />
                    {messageSource.source}
                </a>
            ) : (
                <Tooltip>
                    <TooltipTrigger asChild>
                        <span className="inline-flex items-center gap-1.5 truncate">
                            <FileText
                                className="shrink-0"
                                strokeWidth={1.5}
                                size={20}
                            />
                            <span className="truncate">{messageSource.source}</span>
                        </span>
                    </TooltipTrigger>
                    <TooltipContent>{messageSource.source}</TooltipContent>
                </Tooltip>
            )}
        </div>
    );
}

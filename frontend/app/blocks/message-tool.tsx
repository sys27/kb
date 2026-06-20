import { SearchIcon, Sparkles } from 'lucide-react';
import { useState } from 'react';
import { Button } from '~/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '~/components/ui/card';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import type { Message } from '~/services/messages';

interface MessageToolProps {
    callMessage: Message;
    resultMessage?: Message;
}

interface ToolCall {
    callId: string;
    function: string;
    arguments: Record<string, string>;
    exception?: string | null;
}

interface ToolResult {
    callId: string;
    result: string;
}

interface WebSearchResult {
    url: string;
    title: string;
    chunks: string[];
}

function argumentsToString(toolCall: ToolCall): string {
    return Object.entries(toolCall.arguments)
        .map(([name, value]) => `${name}: ${value}`)
        .join(', ');
}

export function MessageTool({ callMessage, resultMessage }: MessageToolProps) {
    let call = JSON.parse(callMessage.text) as ToolCall;
    let [isOpen, setIsOpen] = useState(false);

    if (call.function === 'web_search') {
        if (!resultMessage) return null;
        let result = JSON.parse(resultMessage.text) as ToolResult;
        let searchResult = JSON.parse(result.result) as WebSearchResult[];
        let args = call.exception ?? argumentsToString(call);

        return (
            <Collapsible
                open={isOpen}
                onOpenChange={setIsOpen}
                className="rounded-2xl border">
                <CollapsibleTrigger asChild>
                    <div className="flex cursor-pointer items-center gap-2 py-2 pl-4 text-sm">
                        <SearchIcon className="size-4" />
                        Web Search
                        {isOpen && (
                            <span className="truncate text-sm text-muted-foreground">{args}</span>
                        )}
                    </div>
                </CollapsibleTrigger>

                <CollapsibleContent className="min-w-0 overflow-hidden px-4 pt-1 pb-4">
                    <div className="flex flex-col gap-2">
                        {searchResult.map((result, index) => (
                            <Card
                                key={index}
                                className="min-w-0">
                                <CardHeader>
                                    <CardTitle>{result.title}</CardTitle>
                                    <CardDescription>
                                        <a
                                            href={result.url}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="block max-w-full overflow-hidden text-ellipsis whitespace-nowrap">
                                            {result.url}
                                        </a>
                                    </CardDescription>
                                </CardHeader>
                                <CardContent className="prose max-w-none border-t px-4 pt-4 text-sm wrap-break-word dark:prose-invert">
                                    {result.chunks.map((chunk, index) => (
                                        <>
                                            <span>Chunk #{index + 1}:</span>
                                            <p
                                                key={index}
                                                className="wrap-anywhere">
                                                {chunk}
                                            </p>
                                        </>
                                    ))}
                                </CardContent>
                            </Card>
                        ))}
                    </div>
                </CollapsibleContent>
            </Collapsible>
        );
    }

    return (
        <Collapsible
            open={isOpen}
            onOpenChange={setIsOpen}>
            <CollapsibleTrigger>
                <Button
                    variant="ghost"
                    size="sm"
                    className="justify-start">
                    <Sparkles />
                    {call.function}
                </Button>
            </CollapsibleTrigger>

            <CollapsibleContent>{resultMessage?.text}</CollapsibleContent>
        </Collapsible>
    );
}

import { Copy } from 'lucide-react';
import { Badge } from '~/components/ui/badge';
import { Button } from '~/components/ui/button';
import {
    Card,
    CardAction,
    CardContent,
    CardFooter,
    CardHeader,
    CardTitle,
} from '~/components/ui/card';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import type { Message } from '~/services/messages';

export default function MessageItem({ message }: { message: Message }) {
    let handleCopy = async () => {
        await navigator.clipboard.writeText(message.text);
    };

    return (
        <Collapsible defaultOpen={message.kind === 'Text'}>
            <Card>
                <CardHeader>
                    <CollapsibleTrigger asChild>
                        <div className="flex w-full cursor-pointer items-center justify-between">
                            <CardTitle>{message.role}</CardTitle>
                            <CardAction>
                                <Badge variant="secondary">{message.kind}</Badge>
                            </CardAction>
                        </div>
                    </CollapsibleTrigger>
                </CardHeader>

                <CollapsibleContent className="flex flex-col gap-4">
                    <CardContent className="whitespace-pre-wrap">{message.text}</CardContent>

                    <CardFooter className="flex flex-row justify-between px-2 py-0">
                        <Button
                            variant="ghost"
                            size="icon"
                            onClick={handleCopy}>
                            <Copy />
                        </Button>
                        <span className="text-muted-foreground">
                            {message.timestamp.toLocaleString()}
                        </span>
                    </CardFooter>
                </CollapsibleContent>
            </Card>
        </Collapsible>
    );
}

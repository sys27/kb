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
import {
    getMessageKindName,
    getMessageRoleName,
    MessageType,
    type Message,
} from '~/services/messages';

export default function MessageItem({ message }: { message: Message }) {
    let handleCopy = async () => {
        await navigator.clipboard.writeText(message.text);
    };
    let isOpen =
        message.messageTypeId == MessageType.assistantAnswerId ||
        message.messageTypeId == MessageType.userRequestId;
    let role = getMessageRoleName(message);
    let kind = getMessageKindName(message);

    return (
        <Collapsible defaultOpen={isOpen}>
            <Card>
                <CardHeader>
                    <CollapsibleTrigger asChild>
                        <div className="flex w-full cursor-pointer items-center justify-between">
                            <CardTitle>{role}</CardTitle>
                            {kind && (
                                <CardAction>
                                    <Badge variant="secondary">{kind}</Badge>
                                </CardAction>
                            )}
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

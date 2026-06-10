import { MessageType, type Message } from '~/services/messages';
import MessageText from './message-text';
import { MessageTool } from './message-tool';

interface MessageItemProps {
    message: Message;
    toolResults: Record<string, Message>;
}

export default function MessageItem({ message, toolResults }: MessageItemProps) {
    if (message.messageTypeId == MessageType.toolCallId) {
        let callData = JSON.parse(message.text) as { callId: string };
        let result = toolResults[callData.callId];

        return (
            <MessageTool
                callMessage={message}
                resultMessage={result}
            />
        );
    }

    if (message.messageTypeId == MessageType.toolResultId) {
        return null;
    }

    return <MessageText message={message} />;
}

import { MessageType, type Message } from '~/services/messages';
import { MessageAnswer } from './message-answer';
import { MessageContext } from './message-context';
import { MessageReasoning } from './message-reasoning';
import { MessageRequest } from './message-request';
import { MessageSource } from './message-source';
import { MessageTool } from './message-tool';

interface MessageItemProps {
    message: Message;
    toolResults: Record<string, Message>;
}

export default function MessageItem({ message, toolResults }: MessageItemProps) {
    if (message.messageTypeId == MessageType.assistantReasoningId) {
        return <MessageReasoning message={message} />;
    }

    if (message.messageTypeId == MessageType.assistantAnswerId) {
        return <MessageAnswer message={message} />;
    }

    if (message.messageTypeId == MessageType.userContextId) {
        return <MessageContext message={message} />;
    }

    if (message.messageTypeId == MessageType.userRequestId) {
        return <MessageRequest message={message} />;
    }

    if (message.messageTypeId == MessageType.toolCallId) {
        let callData: { callId: string };
        try {
            callData = JSON.parse(message.text) as { callId: string };
        } catch {
            return null;
        }

        let result = toolResults[callData.callId];

        return (
            <MessageTool
                callMessage={message}
                resultMessage={result}
            />
        );
    }

    if (message.messageTypeId == MessageType.addSourceId) {
        return <MessageSource message={message} />;
    }

    return null;
}

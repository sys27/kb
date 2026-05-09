import { useForm } from '@tanstack/react-form';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FileText, MessageCircle, Plus, Search, Send } from 'lucide-react';
import { useEffect, useRef } from 'react';
import z from 'zod';
import MessageItem from '~/blocks/message-item';
import MessageSkeletonItem from '~/blocks/message-skeleton-item';
import {
    DropdownMenu,
    DropdownMenuCheckboxItem,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from '~/components/ui/dropdown-menu';
import {
    Empty,
    EmptyDescription,
    EmptyHeader,
    EmptyMedia,
    EmptyTitle,
} from '~/components/ui/empty';
import {
    InputGroup,
    InputGroupAddon,
    InputGroupButton,
    InputGroupTextarea,
} from '~/components/ui/input-group';
import { ScrollArea } from '~/components/ui/scroll-area';
import { Spinner } from '~/components/ui/spinner';
import {
    getMessages,
    messagesOptions,
    sendMessage,
    type Message,
    type MessageSse,
} from '~/services/messages';
import type { Route } from './+types/chat';

let FormSchema = z.object({
    message: z.string().min(1, 'Message is required'),
});

type FormType = z.infer<typeof FormSchema>;

export default function Chat({ params }: Route.ComponentProps) {
    let chatId = Number(params.chatId);
    let queryClient = useQueryClient();
    let messageOptionsForChat = messagesOptions(chatId);
    let { data: messages, isPending } = useQuery(messageOptionsForChat);

    let form = useForm({
        defaultValues: {
            message: '',
        } as FormType,
        validators: {
            onChange: FormSchema,
        },
        onSubmit: values => {
            mutation.mutate(values.value);
        },
    });
    let mutation = useMutation({
        mutationFn: async (form: FormType) => {
            let request = sendMessage(chatId, form.message);
            let lastChunkKey = '';
            let lastChunk: MessageSse | null = null;

            for await (let chunk of request) {
                let chunkKey = `${chunk.role}-${chunk.kind}`;

                queryClient.setQueryData<Message[]>(messageOptionsForChat.queryKey, (prev = []) => {
                    let newPrev = [...prev];

                    if (lastChunk && chunkKey === lastChunkKey) {
                        let lastMsg = newPrev[newPrev.length - 1];
                        if (
                            lastMsg &&
                            lastMsg.role === lastChunk.role &&
                            lastMsg.kind === lastChunk.kind
                        ) {
                            newPrev[newPrev.length - 1] = {
                                ...lastMsg,
                                text: lastMsg.text + chunk.text,
                            };
                        }
                    } else {
                        newPrev.push({
                            ...chunk,
                            id: Date.now(),
                            timestamp: new Date(),
                        });
                    }

                    lastChunkKey = chunkKey;
                    lastChunk = chunk;

                    return newPrev;
                });
            }
        },
        onMutate: async form => {
            await queryClient.cancelQueries(messageOptionsForChat);

            queryClient.setQueryData<Message[]>(messageOptionsForChat.queryKey, (prev = []) => [
                ...prev,
                {
                    id: Date.now(),
                    role: 'User',
                    kind: 'Text',
                    text: form.message,
                    timestamp: new Date(),
                },
            ]);
        },
        onSuccess: () => form.reset(),
        onSettled: async (data, error, variables, onMutateResult, context) => {
            try {
                let serverMessages = await getMessages(chatId);

                queryClient.setQueryData<Message[]>(messageOptionsForChat.queryKey, (prev = []) => {
                    let merged = serverMessages.map(srv => {
                        let match = prev.find(
                            p => p.role === srv.role && p.kind === srv.kind && p.text === srv.text,
                        );

                        if (match) {
                            return { ...match, ...srv };
                        }

                        return srv;
                    });

                    return merged;
                });
            } catch {
                context.client.invalidateQueries(messageOptionsForChat);
            }
        },
    });

    let bottomRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    return (
        <div className="flex h-screen w-full flex-col gap-4 p-2">
            <ScrollArea className="flex-1 overflow-y-auto">
                <div className="flex flex-col gap-2 p-1">
                    {isPending ? (
                        <>
                            <MessageSkeletonItem />
                            <MessageSkeletonItem />
                            <MessageSkeletonItem />
                        </>
                    ) : messages && messages.length > 0 ? (
                        messages.map(message => (
                            <MessageItem
                                key={message.id}
                                message={message}
                            />
                        ))
                    ) : (
                        <Empty>
                            <EmptyHeader>
                                <EmptyMedia variant="icon">
                                    <MessageCircle />
                                </EmptyMedia>
                                <EmptyTitle>No Messages Yet</EmptyTitle>
                                <EmptyDescription>
                                    You haven&apos;t sent any messages yet. Get started by sending
                                    your first message.
                                </EmptyDescription>
                            </EmptyHeader>
                        </Empty>
                    )}
                </div>

                <div ref={bottomRef} />
            </ScrollArea>
            <form
                className="contents"
                onSubmit={e => {
                    e.preventDefault();
                    form.handleSubmit();
                }}>
                <InputGroup className="flex-none">
                    <form.Field
                        name="message"
                        children={field => {
                            const isInvalid =
                                field.state.meta.isTouched && !field.state.meta.isValid;

                            return (
                                <InputGroupTextarea
                                    placeholder="Ask anything..."
                                    className="max-h-20 min-h-20"
                                    id={field.name}
                                    name={field.name}
                                    value={field.state.value}
                                    onBlur={field.handleBlur}
                                    onChange={e => field.handleChange(e.target.value)}
                                    data-invalid={isInvalid}
                                    aria-invalid={isInvalid}
                                    required
                                />
                            );
                        }}
                    />
                    <InputGroupAddon
                        align="block-end"
                        className="flex flex-row justify-between">
                        <InputGroupButton>
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Plus />
                                </DropdownMenuTrigger>
                                <DropdownMenuContent className="w-48">
                                    <DropdownMenuItem disabled>
                                        <FileText />
                                        Add File
                                    </DropdownMenuItem>
                                    <DropdownMenuCheckboxItem checked={true}>
                                        <Search />
                                        Web Search
                                    </DropdownMenuCheckboxItem>
                                </DropdownMenuContent>
                            </DropdownMenu>
                        </InputGroupButton>
                        <InputGroupButton
                            type="submit"
                            disabled={mutation.isPending}>
                            {mutation.isPending ? <Spinner data-icon="inline-start" /> : <Send />}
                        </InputGroupButton>
                    </InputGroupAddon>
                </InputGroup>
            </form>
        </div>
    );
}

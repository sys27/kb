import { useForm } from '@tanstack/react-form';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FileText, Plus, Search, Send, X } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import z from 'zod';
import { AddSourcesDialog } from '~/components/sources/add-sources-dialog';
import FollowUpQuestion from '~/components/follow-up-question';
import { MessagesList } from '~/components/messages/messages-list';
import { Badge } from '~/components/ui/badge';
import { Button } from '~/components/ui/button';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from '~/components/ui/dropdown-menu';
import {
    InputGroup,
    InputGroupAddon,
    InputGroupButton,
    InputGroupTextarea,
} from '~/components/ui/input-group';
import { Kbd } from '~/components/ui/kbd';
import { ScrollArea } from '~/components/ui/scroll-area';
import { Spinner } from '~/components/ui/spinner';
import { followUpQuestionsOptions } from '~/services/chats';
import {
    getMessages,
    messagesOptions,
    MessageType,
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
    let [webSearchEnabled, setWebSearchEnabled] = useState<Map<number, boolean>>(new Map());
    let enableWebSearch = webSearchEnabled.get(chatId) ?? false;
    let [openUploadDialog, setOpenUploadDialog] = useState(false);

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
            let request = sendMessage(chatId, form.message, enableWebSearch);
            let lastChunk: MessageSse | null = null;

            for await (let chunk of request) {
                queryClient.setQueryData<Message[]>(messageOptionsForChat.queryKey, (prev = []) => {
                    let newPrev = [...prev];

                    if (lastChunk && chunk.messageTypeId === lastChunk.messageTypeId) {
                        let lastMsg = newPrev[newPrev.length - 1];
                        if (lastMsg && lastMsg.messageTypeId === lastChunk.messageTypeId) {
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

                    lastChunk = chunk;

                    return newPrev;
                });
            }
        },
        onMutate: async (form, context) => {
            await context.client.cancelQueries(messageOptionsForChat);
            context.client.removeQueries(followUpQuestionsOptions(chatId));

            context.client.setQueryData<Message[]>(messageOptionsForChat.queryKey, (prev = []) => [
                ...prev,
                {
                    id: Date.now(),
                    messageTypeId: MessageType.userRequestId,
                    text: form.message,
                    timestamp: new Date(),
                },
            ]);
        },
        onSuccess: () => form.reset(),
        onSettled: async (data, error, variables, onMutateResult, context) => {
            try {
                let serverMessages = await getMessages(chatId);

                context.client.setQueryData<Message[]>(
                    messageOptionsForChat.queryKey,
                    (prev = []) => {
                        let merged = serverMessages.map(srv => {
                            let match = prev.find(
                                p => p.messageTypeId === srv.messageTypeId && p.text === srv.text,
                            );

                            if (match) {
                                return { ...match, ...srv };
                            }

                            return srv;
                        });

                        return merged;
                    },
                );
            } catch {
                context.client.invalidateQueries(messageOptionsForChat);
            }

            context.client.invalidateQueries(followUpQuestionsOptions(chatId));
        },
    });

    let followUpOptions = followUpQuestionsOptions(chatId);
    let { data: followUpData } = useQuery({
        ...followUpOptions,
        enabled: !!messages && messages.length > 0 && !mutation.isPending,
    });

    let handleFollowUpSubmit = (text: string) => () => {
        form.setFieldValue('message', text);
        form.handleSubmit();
    };

    let bottomRef = useRef<HTMLDivElement>(null);
    let isAtBottomRef = useRef(true);

    let handleScroll = () => {
        let viewport = document.querySelector('[data-slot="scroll-area-viewport"]');
        if (!viewport) {
            return;
        }

        isAtBottomRef.current =
            viewport.scrollTop + viewport.clientHeight >= viewport.scrollHeight - 50;
    };

    useEffect(() => {
        let viewport = document.querySelector('[data-slot="scroll-area-viewport"]');
        viewport?.addEventListener('scroll', handleScroll, { passive: true });

        return () => {
            viewport?.removeEventListener('scroll', handleScroll);
        };
    }, []);

    useEffect(() => {
        if (!isAtBottomRef.current) {
            return;
        }

        bottomRef.current?.scrollIntoView();
    }, [messages, followUpData]);

    return (
        <div className="flex h-screen w-full flex-col gap-4 p-2">
            <ScrollArea className="flex-1 overflow-y-auto">
                <div className="mx-auto flex max-w-3xl flex-col gap-4 p-1">
                    <MessagesList
                        messages={messages}
                        isPending={isPending}
                    />

                    {followUpData?.questions.map(question => (
                        <FollowUpQuestion
                            key={question}
                            text={question}
                            onSubmit={handleFollowUpSubmit(question)}
                        />
                    ))}
                </div>

                <div ref={bottomRef} />
            </ScrollArea>
            {/* TODO: move to separate control */}
            <form
                className="contents"
                onSubmit={e => {
                    e.preventDefault();
                    form.handleSubmit();
                }}>
                <InputGroup className="mx-auto max-w-3xl flex-none">
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
                                    onKeyDown={e => {
                                        if (e.key === 'Enter' && !e.shiftKey) {
                                            e.preventDefault();

                                            if (!mutation.isPending && field.state.value.trim()) {
                                                form.handleSubmit();
                                            }
                                        }
                                    }}
                                    data-invalid={isInvalid}
                                    aria-invalid={isInvalid}
                                    required
                                />
                            );
                        }}
                    />
                    <InputGroupAddon
                        align="block-end"
                        className="flex flex-row">
                        <div className="flex flex-1 flex-row">
                            <InputGroupButton>
                                <DropdownMenu>
                                    <DropdownMenuTrigger>
                                        <Plus />
                                    </DropdownMenuTrigger>
                                    <DropdownMenuContent className="w-48">
                                        <DropdownMenuItem
                                            onSelect={() => setOpenUploadDialog(true)}>
                                            <FileText />
                                            Add Sources
                                        </DropdownMenuItem>
                                        <DropdownMenuItem
                                            onSelect={() =>
                                                setWebSearchEnabled(prev => {
                                                    let next = new Map(prev);
                                                    next.set(chatId, !next.get(chatId) || false);
                                                    return next;
                                                })
                                            }>
                                            <Search />
                                            Web Search
                                        </DropdownMenuItem>
                                    </DropdownMenuContent>
                                </DropdownMenu>
                            </InputGroupButton>
                            {enableWebSearch && (
                                <Badge
                                    variant="default"
                                    className="inline-flex items-center gap-1">
                                    <Search />
                                    Web Search
                                    <Button
                                        variant="ghost"
                                        size="xs"
                                        className="m-0 rounded-full p-0"
                                        onClick={() =>
                                            setWebSearchEnabled(prev => {
                                                let next = new Map(prev);
                                                next.set(chatId, false);
                                                return next;
                                            })
                                        }>
                                        <X />
                                    </Button>
                                </Badge>
                            )}
                        </div>
                        <InputGroupButton
                            type="submit"
                            disabled={mutation.isPending}>
                            {mutation.isPending ? <Spinner data-icon="inline-start" /> : <Send />}
                        </InputGroupButton>
                    </InputGroupAddon>
                </InputGroup>
                <div className="flex flex-col items-center gap-4">
                    <p className="text-sm text-muted-foreground">
                        Use <Kbd>Enter</Kbd> to send, <Kbd>Shift + Enter</Kbd> for a new line.
                    </p>
                </div>
            </form>

            <AddSourcesDialog
                chatId={chatId}
                open={openUploadDialog}
                onOpenChange={setOpenUploadDialog}
            />
        </div>
    );
}

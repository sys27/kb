import { useForm } from '@tanstack/react-form';
import { useMutation } from '@tanstack/react-query';
import z from 'zod';
import { Button } from '~/components/ui/button';
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '~/components/ui/dialog';
import { Field, FieldError, FieldGroup } from '~/components/ui/field';
import { Input } from '~/components/ui/input';
import { Label } from '~/components/ui/label';
import { Spinner } from '~/components/ui/spinner';
import { chatsOptions, updateChat, type Chat } from '~/services/chats';

interface ChatEditDialogProps {
    chat: Chat;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

let FormSchema = z.object({
    chatName: z
        .string()
        .min(1, 'Chat name is required')
        .max(256, 'Chat name must be less than 256 characters'),
});

type FormType = z.infer<typeof FormSchema>;

export default function ChatEditDialog({ chat, open, onOpenChange }: ChatEditDialogProps) {
    let form = useForm({
        defaultValues: {
            chatName: '',
        } as FormType,
        validators: {
            onChange: FormSchema,
        },
        onSubmit: values => {
            mutation.mutate(values.value, {
                onSuccess: () => {
                    onOpenChange(false);
                },
            });
        },
    });
    let mutation = useMutation({
        mutationFn: (form: FormType) => updateChat(chat.id, form.chatName),
        onMutate: async (newChat, context) => {
            await context.client.cancelQueries(chatsOptions);

            let previousChats = context.client.getQueryData(chatsOptions.queryKey);
            if (previousChats) {
                let current = previousChats.find(p => p.id === chat.id);
                if (current) {
                    context.client.setQueryData(
                        chatsOptions.queryKey,
                        previousChats.map(c =>
                            c.id === chat.id ? { ...c, name: newChat.chatName } : c,
                        ),
                    );
                }
            }

            return { previousChats };
        },
        onError: (err, variables, onMutateResult, context) => {
            if (onMutateResult?.previousChats) {
                context.client.setQueryData<Chat[]>(
                    chatsOptions.queryKey,
                    onMutateResult.previousChats,
                );
            }
        },
        onSettled: (data, error, variables, onMutateResult, context) =>
            context.client.invalidateQueries(chatsOptions),
    });

    return (
        <Dialog
            open={open}
            onOpenChange={onOpenChange}>
            <DialogContent>
                <form
                    className="contents"
                    onSubmit={e => {
                        e.preventDefault();
                        form.handleSubmit();
                    }}>
                    <DialogHeader>
                        <DialogTitle>Edit Chat</DialogTitle>
                    </DialogHeader>
                    <FieldGroup>
                        <form.Field
                            name="chatName"
                            children={field => {
                                const isInvalid =
                                    field.state.meta.isTouched && !field.state.meta.isValid;

                                return (
                                    <Field data-invalid={isInvalid}>
                                        <Label htmlFor={field.name}>Name</Label>
                                        <Input
                                            id={field.name}
                                            name={field.name}
                                            value={field.state.value}
                                            onBlur={field.handleBlur}
                                            onChange={e => field.handleChange(e.target.value)}
                                            aria-invalid={isInvalid}
                                            required
                                        />
                                        {isInvalid && (
                                            <FieldError errors={field.state.meta.errors} />
                                        )}
                                    </Field>
                                );
                            }}
                        />
                    </FieldGroup>
                    <DialogFooter>
                        <Button
                            type="submit"
                            disabled={mutation.isPending}>
                            {mutation.isPending && <Spinner data-icon="inline-start" />} Ok
                        </Button>
                        <DialogClose asChild>
                            <Button variant="outline">Cancel</Button>
                        </DialogClose>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}

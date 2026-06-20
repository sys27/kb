import { useForm } from '@tanstack/react-form';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Sparkles } from 'lucide-react';
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
import { Field, FieldError, FieldGroup, FieldLabel } from '~/components/ui/field';
import {
    InputGroup,
    InputGroupAddon,
    InputGroupButton,
    InputGroupInput,
} from '~/components/ui/input-group';
import {
    Select,
    SelectContent,
    SelectGroup,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '~/components/ui/select';
import { Spinner } from '~/components/ui/spinner';
import { chatsOptions, generateName, updateChat, type Chat } from '~/services/chats';
import { projectChatsOptions } from '~/services/project-chats';
import { projectsOptions } from '~/services/projects';

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
    projectId: z.string(),
});

type FormType = z.infer<typeof FormSchema>;

export default function ChatEditDialog({ chat, open, onOpenChange }: ChatEditDialogProps) {
    let { data: projects } = useQuery(projectsOptions);
    let form = useForm({
        defaultValues: {
            chatName: chat.name,
            projectId: chat.projectId?.toString() || '0',
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
        mutationFn: (form: FormType) => {
            let projectId = form.projectId == '0' ? null : parseInt(form.projectId);

            return updateChat(chat.id, form.chatName, projectId);
        },
        onMutate: async (newChat, context) => {
            await context.client.cancelQueries(chatsOptions);

            let previousChats = context.client.getQueryData(chatsOptions.queryKey);
            if (previousChats) {
                let current = previousChats.find(p => p.id === chat.id);
                if (current) {
                    context.client.setQueryData(
                        chatsOptions.queryKey,
                        previousChats.map(c =>
                            c.id === chat.id
                                ? {
                                      ...c,
                                      name: newChat.chatName,
                                      projectId:
                                          newChat.projectId == '0'
                                              ? null
                                              : parseInt(newChat.projectId),
                                  }
                                : c,
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
        onSettled: (data, error, variables, onMutateResult, context) => {
            context.client.invalidateQueries(chatsOptions);

            if (data?.projectId)
                context.client.invalidateQueries(projectChatsOptions(data.projectId));
        },
    });
    let nameMutation = useMutation({
        mutationFn: () => generateName(chat.id),
        onSuccess: newName => {
            form.setFieldValue('chatName', newName);
        },
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
                                        <FieldLabel htmlFor={field.name}>Name</FieldLabel>
                                        <InputGroup>
                                            <InputGroupInput
                                                id={field.name}
                                                name={field.name}
                                                value={field.state.value}
                                                onBlur={field.handleBlur}
                                                onChange={e => field.handleChange(e.target.value)}
                                                aria-invalid={isInvalid}
                                                required
                                            />

                                            <InputGroupAddon align="inline-end">
                                                <InputGroupButton
                                                    type="button"
                                                    variant="ghost"
                                                    className="text-muted-foreground hover:text-foreground"
                                                    disabled={nameMutation.isPending}
                                                    onClick={() => nameMutation.mutate()}>
                                                    {nameMutation.isPending ? (
                                                        <Spinner data-icon="inline-start" />
                                                    ) : (
                                                        <Sparkles className="size-4" />
                                                    )}
                                                </InputGroupButton>
                                            </InputGroupAddon>
                                        </InputGroup>
                                        {isInvalid && (
                                            <FieldError errors={field.state.meta.errors} />
                                        )}
                                    </Field>
                                );
                            }}
                        />

                        <form.Field
                            name="projectId"
                            children={field => {
                                const isInvalid =
                                    field.state.meta.isTouched && !field.state.meta.isValid;

                                return (
                                    <Field data-invalid={isInvalid}>
                                        <FieldLabel htmlFor={field.name}>Project</FieldLabel>
                                        <Select
                                            name={field.name}
                                            value={field.state.value!}
                                            onValueChange={field.handleChange}>
                                            <SelectTrigger
                                                aria-invalid={isInvalid}
                                                className="w-full">
                                                <SelectValue placeholder="Select a project" />
                                            </SelectTrigger>
                                            <SelectContent position="popper">
                                                <SelectGroup>
                                                    <SelectItem
                                                        key={0}
                                                        value={'0'}>
                                                        No project
                                                    </SelectItem>
                                                    {projects?.map(project => (
                                                        <SelectItem
                                                            key={project.id}
                                                            value={project.id.toString()}>
                                                            {project.name}
                                                        </SelectItem>
                                                    ))}
                                                </SelectGroup>
                                            </SelectContent>
                                        </Select>
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

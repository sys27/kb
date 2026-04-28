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
import { Field, FieldError, FieldGroup, FieldLabel } from '~/components/ui/field';
import { Input } from '~/components/ui/input';
import { Spinner } from '~/components/ui/spinner';
import { projectDocumentsOptions, uploadDocument } from '~/services/project-documents';

interface UploadDocumentDialogProps {
    projectId: number;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

let FormSchema = z.object({
    file: z.file().min(1),
});

type FormType = z.infer<typeof FormSchema>;

export default function UploadDocumentDialog({
    projectId,
    open,
    onOpenChange,
}: UploadDocumentDialogProps) {
    let form = useForm({
        defaultValues: {} as FormType,
        validators: {
            onChange: FormSchema,
        },
        onSubmit: ({ value }) => {
            mutation.mutate(value, {
                onSuccess: () => {
                    onOpenChange(false);
                },
            });
        },
    });
    let mutation = useMutation({
        mutationFn: (form: FormType) => uploadDocument(projectId, form.file),
        onSettled: (data, error, variables, onMutateResult, context) =>
            context.client.invalidateQueries(projectDocumentsOptions(projectId)),
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
                        <DialogTitle>Upload Document</DialogTitle>
                    </DialogHeader>
                    <FieldGroup>
                        <form.Field
                            name="file"
                            children={field => {
                                const isInvalid =
                                    field.state.meta.isTouched && !field.state.meta.isValid;

                                return (
                                    <Field data-invalid={isInvalid}>
                                        <FieldLabel htmlFor={field.name}>File</FieldLabel>
                                        <Input
                                            type="file"
                                            id={field.name}
                                            name={field.name}
                                            onBlur={field.handleBlur}
                                            onChange={e => {
                                                let file = e.target.files?.[0];
                                                if (file) {
                                                    field.handleChange(file);
                                                }
                                            }}
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

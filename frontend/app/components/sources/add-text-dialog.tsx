import { useForm } from '@tanstack/react-form';
import z from 'zod';
import { Button } from '../ui/button';
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '../ui/dialog';
import { Field, FieldError } from '../ui/field';
import { Textarea } from '../ui/textarea';

interface AddTextDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    addText: (text: string) => void;
}

let FormSchema = z.object({
    text: z
        .string()
        .min(1)
        .max(64 * 1024),
});

type FormTypeInput = z.input<typeof FormSchema>;

export function AddTextDialog({ open, onOpenChange, addText }: AddTextDialogProps) {
    let form = useForm({
        defaultValues: { text: '' } as FormTypeInput,
        validators: {
            onChange: FormSchema,
        },
        onSubmit: ({ value }) => {
            addText(value.text);
            onOpenChange(false);
        },
    });

    return (
        <Dialog
            open={open}
            onOpenChange={onOpenChange}>
            <DialogContent className="min-w-lg">
                <form
                    className="contents"
                    onSubmit={e => {
                        e.preventDefault();
                        form.handleSubmit();
                    }}>
                    <DialogHeader>
                        <DialogTitle>Add Text</DialogTitle>
                    </DialogHeader>

                    <div className="flex h-full w-full flex-col gap-2">
                        <form.Field
                            name="text"
                            children={field => {
                                const isInvalid =
                                    field.state.meta.isTouched && !field.state.meta.isValid;

                                return (
                                    <Field data-invalid={isInvalid}>
                                        <Textarea
                                            className="max-h-56 resize-none"
                                            placeholder="Enter text..."
                                            autoComplete="off"
                                            id={field.name}
                                            name={field.name}
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
                        <span className="text-center text-sm text-muted-foreground">
                            Enter text to be saved as a document.
                        </span>
                    </div>

                    <DialogFooter>
                        <Button type="submit">Ok</Button>
                        <DialogClose asChild>
                            <Button variant="outline">Cancel</Button>
                        </DialogClose>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}

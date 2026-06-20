import { useForm } from '@tanstack/react-form';
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
import { Field, FieldError } from '~/components/ui/field';
import { Textarea } from '~/components/ui/textarea';

interface AddWebSitesDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    addWebSites: (webSites: string[]) => void;
}

let FormSchema = z.object({
    webSites: z
        .string()
        .min(1)
        .transform(value =>
            value
                .split('\n')
                .map(site => site.trim())
                .filter(site => site),
        ),
});

type FormTypeInput = z.input<typeof FormSchema>;

export function AddWebSitesDialog({ open, onOpenChange, addWebSites }: AddWebSitesDialogProps) {
    let form = useForm({
        defaultValues: { webSites: '' } as FormTypeInput,
        validators: {
            onChange: FormSchema,
        },
        onSubmit: ({ value }) => {
            let model = FormSchema.parse(value);
            addWebSites(model.webSites);
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
                        <DialogTitle>Add Web Sites</DialogTitle>
                    </DialogHeader>

                    <div className="flex h-full w-full flex-col gap-2">
                        <form.Field
                            name="webSites"
                            children={field => {
                                const isInvalid =
                                    field.state.meta.isTouched && !field.state.meta.isValid;

                                return (
                                    <Field data-invalid={isInvalid}>
                                        <Textarea
                                            className="max-h-56 resize-none"
                                            placeholder="Enter web sites..."
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
                            Enter one web site per line.
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

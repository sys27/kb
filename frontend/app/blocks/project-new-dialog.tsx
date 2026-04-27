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
import { createProject, projectsOptions, type Project } from '~/services/projects';

interface ProjectNewDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

let FormSchema = z.object({
    projectName: z
        .string()
        .min(1, 'Project name is required')
        .max(256, 'Project name must be less than 256 characters'),
});

type FormType = z.infer<typeof FormSchema>;

export default function ProjectNewDialog({ open, onOpenChange }: ProjectNewDialogProps) {
    let form = useForm({
        defaultValues: {
            projectName: '',
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
        mutationFn: (project: FormType) => createProject(project.projectName),
        onMutate: async (newProject, context) => {
            await context.client.cancelQueries(projectsOptions);

            let previousProjects = context.client.getQueryData(projectsOptions.queryKey);
            if (previousProjects) {
                context.client.setQueryData(projectsOptions.queryKey, [
                    { id: Math.random(), name: newProject.projectName },
                    ...previousProjects,
                ]);
            }

            return { previousProjects };
        },
        onError: (err, variables, onMutateResult, context) => {
            if (onMutateResult?.previousProjects) {
                context.client.setQueryData<Project[]>(
                    projectsOptions.queryKey,
                    onMutateResult.previousProjects,
                );
            }
        },
        onSettled: (data, error, variables, onMutateResult, context) =>
            context.client.invalidateQueries(projectsOptions),
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
                        <DialogTitle>Create Project</DialogTitle>
                    </DialogHeader>
                    <FieldGroup>
                        <form.Field
                            name="projectName"
                            children={field => {
                                const isInvalid =
                                    field.state.meta.isTouched && !field.state.meta.isValid;

                                return (
                                    <Field data-invalid={isInvalid}>
                                        <FieldLabel htmlFor={field.name}>Name</FieldLabel>
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

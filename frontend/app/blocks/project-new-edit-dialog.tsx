import { Button } from '~/components/ui/button';
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '~/components/ui/dialog';
import { Field, FieldGroup } from '~/components/ui/field';
import { Input } from '~/components/ui/input';
import { Label } from '~/components/ui/label';
import type { Project } from '~/services/projects';

interface ProjectNewEditDialogProps {
    project?: Project;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export default function ProjectNewEditDialog({
    project,
    open,
    onOpenChange,
}: ProjectNewEditDialogProps) {
    return (
        <Dialog
            open={open}
            onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{project ? 'Edit' : 'Create'} Project</DialogTitle>
                </DialogHeader>
                <FieldGroup>
                    <Field>
                        <Label htmlFor="projectName">Name</Label>
                        <Input
                            id="projectName"
                            name="projectName"
                            defaultValue={project?.name}
                            required
                        />
                    </Field>
                </FieldGroup>
                <DialogFooter>
                    <Button type="submit">Ok</Button>
                    <DialogClose asChild>
                        <Button variant="outline">Cancel</Button>
                    </DialogClose>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}

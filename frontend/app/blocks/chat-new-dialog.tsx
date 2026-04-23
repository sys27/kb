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
import {
    Select,
    SelectContent,
    SelectGroup,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '~/components/ui/select';
import type { Project } from '~/services/projects';

interface ChatNewDialogProps {
    projects: Project[];
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export default function ChatNewDialog({ projects, open, onOpenChange }: ChatNewDialogProps) {
    return (
        <Dialog
            open={open}
            onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Create Chat</DialogTitle>
                </DialogHeader>
                <FieldGroup>
                    <Field>
                        <Label htmlFor="chatName">Name</Label>
                        <Input
                            id="chatName"
                            name="name"
                            required
                        />
                    </Field>
                    <Field>
                        <Label htmlFor="projectId">Project</Label>
                        <Select>
                            <SelectTrigger className="w-full">
                                <SelectValue placeholder="Select a project" />
                            </SelectTrigger>
                            <SelectContent position="popper">
                                <SelectGroup>
                                    {projects.map(project => (
                                        <SelectItem
                                            key={project.id}
                                            value={project.id.toString()}>
                                            {project.name}
                                        </SelectItem>
                                    ))}
                                </SelectGroup>
                            </SelectContent>
                        </Select>
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

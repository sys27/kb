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
import type { Chat } from '~/services/chats';

interface ChatEditDialogProps {
    chat: Chat;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export default function ChatEditDialog({ chat, open, onOpenChange }: ChatEditDialogProps) {
    return (
        <Dialog
            open={open}
            onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Edit Chat</DialogTitle>
                </DialogHeader>
                <FieldGroup>
                    <Field>
                        <Label htmlFor="chatName">Name</Label>
                        <Input
                            id="chatName"
                            name="name"
                            defaultValue={chat.name}
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

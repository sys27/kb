import { Ellipsis, MessageCircle, Pencil, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router';
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from '~/components/ui/alert-dialog';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from '~/components/ui/dropdown-menu';
import { SidebarMenuAction, SidebarMenuButton, SidebarMenuItem } from '~/components/ui/sidebar';
import type { Chat } from '~/services/chats';
import ChatEditDialog from './chat-edit-dialog';

export default function ChatMenuItem({ chat }: { chat: Chat }) {
    let [openDeleteDialog, setOpenDeleteDialog] = useState(false);
    let [openEditDialog, setOpenEditDialog] = useState(false);

    return (
        <SidebarMenuItem
            key={chat.id}
            className="group/chat">
            <SidebarMenuButton asChild>
                <Link to={`/chats/${chat.id}`}>
                    <MessageCircle />
                    {chat.name}
                </Link>
            </SidebarMenuButton>
            <SidebarMenuAction className="opacity-0 transition-opacity group-hover/chat:opacity-100">
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Ellipsis />
                    </DropdownMenuTrigger>
                    <DropdownMenuContent>
                        <DropdownMenuItem onSelect={() => setOpenEditDialog(true)}>
                            <Pencil />
                            Edit
                        </DropdownMenuItem>
                        <DropdownMenuItem
                            variant="destructive"
                            onSelect={() => setOpenDeleteDialog(true)}>
                            <Trash2 />
                            Delete
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>

                <ChatEditDialog
                    chat={chat}
                    open={openEditDialog}
                    onOpenChange={setOpenEditDialog}
                />

                <AlertDialog
                    open={openDeleteDialog}
                    onOpenChange={setOpenDeleteDialog}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                            <AlertDialogDescription>
                                This action cannot be undone. This will permanently delete the chat
                                and all of its messages.
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogAction variant="destructive">Delete</AlertDialogAction>
                            <AlertDialogCancel>Cancel</AlertDialogCancel>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            </SidebarMenuAction>
        </SidebarMenuItem>
    );
}

import { useMutation } from '@tanstack/react-query';
import { Ellipsis, Folder, Pencil, Trash2 } from 'lucide-react';
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
import {
    SidebarMenuAction,
    SidebarMenuButton,
    SidebarMenuItem,
    SidebarMenuSub,
} from '~/components/ui/sidebar';
import { Spinner } from '~/components/ui/spinner';
import type { Chat } from '~/services/chats';
import { deleteProject, projectsOptions, type Project } from '~/services/projects';
import ChatMenuItem from './chat-menu-item';
import ProjectEditDialog from './project-edit-dialog';

interface ProjectMenuItemProps {
    project: Project;
    chats: Chat[];
}

export default function ProjectMenuItem({ project, chats }: ProjectMenuItemProps) {
    let [openEditDialog, setOpenEditDialog] = useState(false);
    let [openDeleteDialog, setOpenDeleteDialog] = useState(false);
    let deleteMutation = useMutation({
        mutationFn: () => deleteProject(project.id),
        onSuccess: () => {
            setOpenDeleteDialog(false);
        },
        onSettled: (data, error, variables, onMutateResult, context) =>
            context.client.invalidateQueries(projectsOptions),
    });

    return (
        <SidebarMenuItem
            key={project.id}
            className="group/project">
            <SidebarMenuButton asChild>
                <Link to={`/projects/${project.id}`}>
                    <Folder />
                    {project.name}
                </Link>
            </SidebarMenuButton>
            <SidebarMenuAction className="opacity-0 transition-opacity group-hover/project:opacity-100">
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

                <ProjectEditDialog
                    project={project}
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
                                This action cannot be undone. This will permanently delete the
                                project and all of its chats and messages.
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogAction
                                variant="destructive"
                                onClick={e => {
                                    e.preventDefault();
                                    deleteMutation.mutate();
                                }}
                                disabled={deleteMutation.isPending}>
                                {deleteMutation.isPending && <Spinner data-icon="inline-start" />}
                                Delete
                            </AlertDialogAction>
                            <AlertDialogCancel>Cancel</AlertDialogCancel>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            </SidebarMenuAction>
            <SidebarMenuSub>
                {chats
                    .filter(x => x.projectId === project.id)
                    .map(chat => (
                        <ChatMenuItem
                            key={chat.id}
                            chat={chat}
                        />
                    ))}
            </SidebarMenuSub>
        </SidebarMenuItem>
    );
}

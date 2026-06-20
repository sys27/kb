import { useMutation, useQuery } from '@tanstack/react-query';
import { Ellipsis, FileText, Folder, Pencil, Trash2 } from 'lucide-react';
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
    DropdownMenuGroup,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from '~/components/ui/dropdown-menu';
import {
    SidebarMenuAction,
    SidebarMenuButton,
    SidebarMenuItem,
    SidebarMenuSub,
} from '~/components/ui/sidebar';
import { Spinner } from '~/components/ui/spinner';
import { chatsOptions } from '~/services/chats';
import { deleteProject, projectsOptions, type Project } from '~/services/projects';
import { AddSourcesDialog } from './sources/add-sources-dialog';
import ChatMenuItem from './chat-menu-item';
import ProjectEditDialog from './project-edit-dialog';

interface ProjectMenuItemProps {
    project: Project;
}

export default function ProjectMenuItem({ project }: ProjectMenuItemProps) {
    let { data: chats } = useQuery(chatsOptions);
    let [openUploadDialog, setOpenUploadDialog] = useState(false);
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
                        <DropdownMenuGroup>
                            <DropdownMenuItem onSelect={() => setOpenUploadDialog(true)}>
                                <FileText />
                                Upload File
                            </DropdownMenuItem>
                        </DropdownMenuGroup>
                        <DropdownMenuSeparator />
                        <DropdownMenuGroup>
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
                        </DropdownMenuGroup>
                    </DropdownMenuContent>
                </DropdownMenu>

                <AddSourcesDialog
                    projectId={project.id}
                    open={openUploadDialog}
                    onOpenChange={setOpenUploadDialog}
                />

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
                    ?.filter(x => x.projectId === project.id)
                    .slice(0, 5)
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

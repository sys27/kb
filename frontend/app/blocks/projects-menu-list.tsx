import { FolderPlus } from 'lucide-react';
import { useState } from 'react';
import { CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import {
    SidebarGroup,
    SidebarGroupAction,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarMenu,
} from '~/components/ui/sidebar';
import type { Chat } from '~/services/chats';
import type { Project } from '~/services/projects';
import ProjectMenuItem from './project-menu-item';
import ProjectNewEditDialog from './project-new-edit-dialog';

interface ProjectsMenuListProps {
    projects: Project[];
    chats: Chat[];
}

export default function ProjectsMenuList({ projects, chats }: ProjectsMenuListProps) {
    let [openNewDialog, setOpenNewDialog] = useState(false);

    return (
        <SidebarGroup>
            <SidebarGroupLabel asChild>
                <CollapsibleTrigger>Projects</CollapsibleTrigger>
            </SidebarGroupLabel>
            <SidebarGroupAction>
                <FolderPlus onClick={() => setOpenNewDialog(true)} />

                <ProjectNewEditDialog
                    open={openNewDialog}
                    onOpenChange={setOpenNewDialog}
                />
            </SidebarGroupAction>
            <CollapsibleContent>
                <SidebarGroupContent>
                    <SidebarMenu>
                        {projects.map(project => (
                            <ProjectMenuItem
                                key={project.id}
                                project={project}
                                chats={chats}
                            />
                        ))}
                    </SidebarMenu>
                </SidebarGroupContent>
            </CollapsibleContent>
        </SidebarGroup>
    );
}

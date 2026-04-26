import { ChevronRight, FolderPlus } from 'lucide-react';
import { useState } from 'react';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
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
import ProjectNewDialog from './project-new-dialog';

interface ProjectsMenuListProps {
    projects: Project[];
    chats: Chat[];
}

export default function ProjectsMenuList({ projects, chats }: ProjectsMenuListProps) {
    let [openNewDialog, setOpenNewDialog] = useState(false);

    return (
        <Collapsible
            defaultOpen
            className="group/collapsible">
            <SidebarGroup>
                <SidebarGroupLabel asChild>
                    <CollapsibleTrigger>
                        <ChevronRight className="transition-transform group-data-[state=open]/collapsible:rotate-90" />
                        Projects
                    </CollapsibleTrigger>
                </SidebarGroupLabel>
                <SidebarGroupAction>
                    <FolderPlus onClick={() => setOpenNewDialog(true)} />

                    <ProjectNewDialog
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
        </Collapsible>
    );
}

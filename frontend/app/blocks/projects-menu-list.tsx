import { useQuery } from '@tanstack/react-query';
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
import { Skeleton } from '~/components/ui/skeleton';
import { projectsOptions } from '~/services/projects';
import ProjectMenuItem from './project-menu-item';
import ProjectNewDialog from './project-new-dialog';

export default function ProjectsMenuList() {
    let { data: projects, isPending } = useQuery(projectsOptions);
    let [openNewDialog, setOpenNewDialog] = useState(false);

    return (
        <Collapsible
            defaultOpen
            className="group/collapsible">
            {isPending ? (
                <SidebarGroup>
                    <SidebarGroupLabel>Projects</SidebarGroupLabel>
                    <SidebarGroupContent>
                        <SidebarMenu>
                            <Skeleton className="h-8 w-full" />
                            <Skeleton className="h-8 w-full" />
                            <Skeleton className="h-8 w-full" />
                        </SidebarMenu>
                    </SidebarGroupContent>
                </SidebarGroup>
            ) : (
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
                                {projects?.map(project => (
                                    <ProjectMenuItem
                                        key={project.id}
                                        project={project}
                                    />
                                ))}
                            </SidebarMenu>
                        </SidebarGroupContent>
                    </CollapsibleContent>
                </SidebarGroup>
            )}
        </Collapsible>
    );
}

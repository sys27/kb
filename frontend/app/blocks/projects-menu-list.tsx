import { useQuery } from '@tanstack/react-query';
import { ChevronRight } from 'lucide-react';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '~/components/ui/collapsible';
import {
    SidebarGroup,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarMenu,
} from '~/components/ui/sidebar';
import { Skeleton } from '~/components/ui/skeleton';
import { projectsOptions } from '~/services/projects';
import ProjectMenuItem from './project-menu-item';

export default function ProjectsMenuList() {
    let { data: projects, isPending } = useQuery(projectsOptions);

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

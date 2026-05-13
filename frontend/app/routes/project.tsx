import { useQuery } from '@tanstack/react-query';
import {
    FileText,
    Folder,
    List,
    MessageCircle,
    NotebookText,
    UserCheck,
    UserStar,
} from 'lucide-react';
import ProjectChatsList from '~/blocks/project-chats-list';
import { ProjectDecisionsList } from '~/blocks/project-decisions-list';
import ProjectDocumentsList from '~/blocks/project-documents-list';
import { ProjectFactsList } from '~/blocks/project-facts-list';
import { ProjectPreferencesList } from '~/blocks/project-preferences-list';
import { ProjectTopicsList } from '~/blocks/project-topics-list';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '~/components/ui/tabs';
import { projectsOptions } from '~/services/projects';
import type { Route } from './+types/project';

export default function Project({ params }: Route.ComponentProps) {
    let { data: project } = useQuery(projectsOptions);
    let projectId = Number(params.projectId);
    let projectName = project?.find(p => p.id === projectId)?.name || 'Project Name';

    return (
        <div className="mx-auto md:max-w-3xl">
            <h1 className="flex flex-row items-center gap-2 py-8 text-2xl font-semibold">
                <Folder />
                {projectName}
            </h1>

            <Tabs defaultValue="chats">
                <TabsList
                    variant="line"
                    className="mx-auto flex w-fit justify-center">
                    <TabsTrigger value="chats">
                        <MessageCircle />
                        Chats
                    </TabsTrigger>
                    <TabsTrigger value="documents">
                        <FileText />
                        Documents
                    </TabsTrigger>
                    <TabsTrigger value="topics">
                        <List />
                        Topics
                    </TabsTrigger>
                    <TabsTrigger value="facts">
                        <NotebookText />
                        Facts
                    </TabsTrigger>
                    <TabsTrigger value="decisions">
                        <UserCheck />
                        Decisions
                    </TabsTrigger>
                    <TabsTrigger value="preferences">
                        <UserStar />
                        User Preferences
                    </TabsTrigger>
                </TabsList>

                <TabsContent value="chats">
                    <ProjectChatsList projectId={projectId} />
                </TabsContent>
                <TabsContent value="documents">
                    <ProjectDocumentsList projectId={projectId} />
                </TabsContent>
                <TabsContent value="topics">
                    <ProjectTopicsList projectId={projectId} />
                </TabsContent>
                <TabsContent value="facts">
                    <ProjectFactsList projectId={projectId} />
                </TabsContent>
                <TabsContent value="decisions">
                    <ProjectDecisionsList projectId={projectId} />
                </TabsContent>
                <TabsContent value="preferences">
                    <ProjectPreferencesList projectId={projectId} />
                </TabsContent>
            </Tabs>
        </div>
    );
}

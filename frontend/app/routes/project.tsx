import { useQuery } from '@tanstack/react-query';
import { FileText, Folder, MessageCircle } from 'lucide-react';
import { Item, ItemContent, ItemDescription, ItemMedia, ItemTitle } from '~/components/ui/item';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '~/components/ui/tabs';
import { projectChatsOptions } from '~/services/project-chats';
import { projectDocumentsOptions } from '~/services/project-documents';
import { projectsOptions } from '~/services/projects';
import type { Route } from './+types/project';

export default function Project({ params }: Route.ComponentProps) {
    let projectId = Number(params.projectId);
    let { data: project } = useQuery(projectsOptions);
    let { data: chats } = useQuery(projectChatsOptions(projectId));
    let { data: documents } = useQuery(projectDocumentsOptions(projectId));
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
                </TabsList>

                <TabsContent value="chats">
                    <div className="flex flex-col gap-2 p-2">
                        {chats && chats.length > 0 ? (
                            chats.map(chat => (
                                <Item
                                    key={chat.id}
                                    variant="outline">
                                    <ItemMedia variant="icon">
                                        <MessageCircle />
                                    </ItemMedia>
                                    <ItemContent>
                                        <ItemTitle>{chat.name}</ItemTitle>
                                        <ItemDescription>{chat.lastMessage}</ItemDescription>
                                    </ItemContent>
                                    <ItemContent>
                                        <ItemDescription>
                                            {chat.lastMessageAt
                                                ? new Date(chat.lastMessageAt).toLocaleDateString()
                                                : null}
                                        </ItemDescription>
                                    </ItemContent>
                                </Item>
                            ))
                        ) : (
                            <div className="p-4 text-center text-sm text-muted-foreground">
                                No chats found.
                            </div>
                        )}
                    </div>
                </TabsContent>
                <TabsContent value="documents">
                    <div className="flex flex-col gap-2 p-2">
                        {documents && documents.length > 0 ? (
                            documents.map(document => (
                                <Item
                                    key={document.id}
                                    variant="outline">
                                    <ItemMedia variant="icon">
                                        <FileText />
                                    </ItemMedia>
                                    <ItemContent>
                                        <ItemTitle>{document.name}</ItemTitle>
                                    </ItemContent>
                                    <ItemContent>
                                        <ItemDescription>
                                            {document.lastModifiedAt
                                                ? new Date(
                                                      document.lastModifiedAt,
                                                  ).toLocaleDateString()
                                                : null}
                                        </ItemDescription>
                                    </ItemContent>
                                </Item>
                            ))
                        ) : (
                            <div className="p-4 text-center text-sm text-muted-foreground">
                                No documents found.
                            </div>
                        )}
                    </div>
                </TabsContent>
            </Tabs>
        </div>
    );
}

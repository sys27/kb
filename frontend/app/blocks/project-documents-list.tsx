import { useQuery } from '@tanstack/react-query';
import { FileText } from 'lucide-react';
import { Badge } from '~/components/ui/badge';
import { Item, ItemContent, ItemDescription, ItemMedia, ItemTitle } from '~/components/ui/item';
import { projectDocumentsOptions } from '~/services/project-documents';

interface ProjectDocumentsListProps {
    projectId: number;
}

export default function ProjectDocumentsList({ projectId }: ProjectDocumentsListProps) {
    let { data: documents } = useQuery(projectDocumentsOptions(projectId));

    return (
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
                                <Badge
                                    variant={
                                        document.status === 'Pending'
                                            ? 'default'
                                            : document.status === 'Ingested'
                                              ? 'secondary'
                                              : 'destructive'
                                    }>
                                    {document.status}
                                </Badge>
                            </ItemDescription>
                        </ItemContent>
                        <ItemContent>
                            <ItemDescription>
                                {document.lastModifiedAt
                                    ? document.lastModifiedAt.toLocaleString()
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
    );
}

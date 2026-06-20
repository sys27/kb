import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const ProjectDocumentSchema = z.object({
    id: z.number(),
    name: z.string(),
    lastModifiedAt: z.coerce.date().nullable(),
    status: z.enum(['Pending', 'Ingested', 'Failed']),
});

export type ProjectDocument = z.infer<typeof ProjectDocumentSchema>;

export function projectDocumentsOptions(projectId: number) {
    return queryOptions({
        queryKey: ['project', projectId, 'documents'],
        queryFn: () => getProjectDocuments(projectId),
        staleTime: Infinity,
    });
}

export async function getProjectDocuments(projectId: number): Promise<ProjectDocument[]> {
    let response = await fetch(`/api/projects/${projectId}/documents`);
    if (!response.ok) {
        throw new Error('Failed to fetch project documents');
    }

    let json = await response.json();

    return z.array(ProjectDocumentSchema).parse(json);
}

export async function projectUploadDocument(projectId: number, file: File): Promise<void> {
    let formData = new FormData();
    formData.append('file', file);

    let response = await fetch(`/api/projects/${projectId}/documents/upload`, {
        method: 'POST',
        body: formData,
    });
    if (!response.ok) {
        throw new Error('Failed to upload the document');
    }
}

export async function projectAddWebSite(projectId: number, url: string): Promise<void> {
    let response = await fetch(`/api/projects/${projectId}/documents/add-web-site`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ url }),
    });
    if (!response.ok) {
        throw new Error('Failed to add web site');
    }
}

import { queryOptions } from "@tanstack/react-query";

export interface ProjectDocument {
    id: number;
    name: string;
    lastModifiedAt: string;
}

export function projectDocumentsOptions(projectId: number) {
    return queryOptions({
        queryKey: ["project", projectId, "documents"],
        queryFn: () => getProjectDocuments(projectId),
    });
}

export async function getProjectDocuments(projectId: number): Promise<ProjectDocument[]> {
    let response = await fetch(`/api/projects/${projectId}/documents`);
    if (!response.ok) {
        throw new Error("Failed to fetch project documents");
    }

    return response.json();
}
import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const ProjectFactSchema = z.object({
    id: z.number(),
    name: z.string(),
    facts: z.array(
        z.object({
            id: z.number(),
            name: z.string(),
        }),
    ),
});

export type ProjectFact = z.infer<typeof ProjectFactSchema>;

export function projectFactsOptions(projectId: number) {
    return queryOptions({
        queryKey: ['project', projectId, 'facts'],
        queryFn: () => getProjectFacts(projectId),
    });
}

export async function getProjectFacts(projectId: number): Promise<ProjectFact[]> {
    let response = await fetch(`/api/projects/${projectId}/facts`);
    if (!response.ok) {
        throw new Error('Failed to fetch project facts');
    }

    let json = await response.json();

    return z.array(ProjectFactSchema).parse(json);
}

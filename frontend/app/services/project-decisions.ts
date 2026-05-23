import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const ProjectDecisionSchema = z.object({
    id: z.number(),
    name: z.string(),
    decisions: z.array(
        z.object({
            decision: z.string(),
            reason: z.string(),
        }),
    ),
});

export type ProjectDecision = z.infer<typeof ProjectDecisionSchema>;

export function projectDecisionsOptions(projectId: number) {
    return queryOptions({
        queryKey: ['project', projectId, 'decisions'],
        queryFn: () => getProjectDecisions(projectId),
    });
}

export async function getProjectDecisions(projectId: number): Promise<ProjectDecision[]> {
    let response = await fetch(`/api/projects/${projectId}/decisions`);
    if (!response.ok) {
        throw new Error('Failed to fetch project decisions');
    }

    let json = await response.json();

    return z.array(ProjectDecisionSchema).parse(json);
}

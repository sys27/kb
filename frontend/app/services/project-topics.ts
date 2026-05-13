import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const ProjectTopicSchema = z.object({
    id: z.number(),
    topic: z.string(),
    chat: z.object({
        id: z.number(),
        name: z.string(),
    }),
});

export type ProjectTopic = z.infer<typeof ProjectTopicSchema>;

export function projectTopicsOptions(projectId: number) {
    return queryOptions({
        queryKey: ['project', projectId, 'topics'],
        queryFn: () => getProjectTopics(projectId),
    });
}

export async function getProjectTopics(projectId: number): Promise<ProjectTopic[]> {
    let response = await fetch(`/api/projects/${projectId}/topics`);
    if (!response.ok) {
        throw new Error('Failed to fetch project topics');
    }

    let json = await response.json();

    return z.array(ProjectTopicSchema).parse(json);
}

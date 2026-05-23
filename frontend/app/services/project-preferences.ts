import { queryOptions } from '@tanstack/react-query';
import z from 'zod';

const ProjectPreferenceSchema = z.object({
    id: z.number(),
    name: z.string(),
    userPreferences: z.array(
        z.object({
            id: z.number(),
            name: z.string(),
        }),
    ),
});

export type ProjectPreference = z.infer<typeof ProjectPreferenceSchema>;

export function projectPreferencesOptions(projectId: number) {
    return queryOptions({
        queryKey: ['project', projectId, 'preferences'],
        queryFn: () => getProjectPreferences(projectId),
    });
}

export async function getProjectPreferences(projectId: number): Promise<ProjectPreference[]> {
    let response = await fetch(`/api/projects/${projectId}/preferences`);
    if (!response.ok) {
        throw new Error('Failed to fetch project preferences');
    }

    let json = await response.json();

    return z.array(ProjectPreferenceSchema).parse(json);
}

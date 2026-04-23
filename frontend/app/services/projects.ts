export interface Project {
    get id(): number;
    get name(): string;
}

export async function getProjects(): Promise<Project[]> {
    let response = await fetch("/api/projects");
    if (!response.ok) {
        throw new Error("Failed to fetch projects");
    }

    return response.json();
}
import { queryOptions } from "@tanstack/react-query";

export interface Project {
    get id(): number;
    get name(): string;
}

export const projectsOptions = queryOptions({
    queryKey: ["projects"],
    queryFn: getProjects,
    staleTime: Infinity,
});

export async function getProjects(): Promise<Project[]> {
    let response = await fetch("/api/projects");
    if (!response.ok) {
        throw new Error("Failed to fetch projects");
    }

    return response.json();
}

export async function createProject(name: string): Promise<Project> {
    let response = await fetch("/api/projects", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ name }),
    });
    if (!response.ok) {
        throw new Error("Failed to create project");
    }

    return response.json();
}

export async function updateProject(id: number, name: string): Promise<Project> {
    let response = await fetch(`/api/projects/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ name }),
    });
    if (!response.ok) {
        throw new Error("Failed to update project");
    }

    return response.json();
}

export async function deleteProject(id: number): Promise<void> {
    let response = await fetch(`/api/projects/${id}`, {
        method: "DELETE",
    });
    if (!response.ok) {
        throw new Error("Failed to delete project");
    }
}
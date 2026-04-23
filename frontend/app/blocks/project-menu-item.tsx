import { Ellipsis, Folder } from 'lucide-react';
import { Link } from 'react-router';
import {
    SidebarMenuAction,
    SidebarMenuButton,
    SidebarMenuItem,
    SidebarMenuSub,
} from '~/components/ui/sidebar';
import type { Chat } from '~/services/chats';
import type { Project } from '~/services/projects';
import ChatMenuItem from './chat-menu-item';

export default function ProjectMenuItem({ project, chats }: { project: Project; chats: Chat[] }) {
    return (
        <SidebarMenuItem
            key={project.id}
            className="group/project">
            <SidebarMenuButton asChild>
                <Link to={`/projects/${project.id}`}>
                    <Folder />
                    {project.name}
                </Link>
            </SidebarMenuButton>
            <SidebarMenuAction className="opacity-0 transition-opacity group-hover/project:opacity-100">
                <Ellipsis />
            </SidebarMenuAction>
            <SidebarMenuSub>
                {chats
                    .filter(x => x.projectId === project.id)
                    .map(chat => (
                        <ChatMenuItem
                            key={chat.id}
                            chat={chat}
                        />
                    ))}
            </SidebarMenuSub>
        </SidebarMenuItem>
    );
}

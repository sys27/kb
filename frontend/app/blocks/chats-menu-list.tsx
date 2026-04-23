import { MessageCirclePlus } from 'lucide-react';
import { useState } from 'react';
import {
    SidebarGroup,
    SidebarGroupAction,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarMenu,
} from '~/components/ui/sidebar';
import type { Chat } from '~/services/chats';
import type { Project } from '~/services/projects';
import ChatMenuItem from './chat-menu-item';
import ChatNewDialog from './chat-new-dialog';

interface ChatsMenuListProps {
    projects: Project[];
    chats: Chat[];
}

export default function ChatsMenuList({ projects, chats }: ChatsMenuListProps) {
    let [openNewDialog, setOpenNewDialog] = useState(false);

    return (
        <SidebarGroup>
            <SidebarGroupLabel>Chats</SidebarGroupLabel>
            <SidebarGroupAction>
                <MessageCirclePlus onClick={() => setOpenNewDialog(true)} />

                <ChatNewDialog
                    projects={projects}
                    open={openNewDialog}
                    onOpenChange={setOpenNewDialog}
                />
                {/* <Dialog>
                    <DialogTrigger asChild>
                        <MessageCirclePlus />
                    </DialogTrigger>
                    <DialogContent>
                        <DialogHeader>
                            <DialogTitle>Create Chat</DialogTitle>
                        </DialogHeader>
                        <FieldGroup>
                            <Field>
                                <Label htmlFor="chatName">Name</Label>
                                <Input
                                    id="chatName"
                                    name="name"
                                />
                            </Field>
                            <Field>
                                <Label htmlFor="projectId">Project</Label>
                                <Select>
                                    <SelectTrigger className="w-full">
                                        <SelectValue placeholder="Select a project" />
                                    </SelectTrigger>
                                    <SelectContent position="popper">
                                        <SelectGroup>
                                            {projects.map(project => (
                                                <SelectItem
                                                    key={project.id}
                                                    value={project.id.toString()}>
                                                    {project.name}
                                                </SelectItem>
                                            ))}
                                        </SelectGroup>
                                    </SelectContent>
                                </Select>
                            </Field>
                        </FieldGroup>
                        <DialogFooter>
                            <Button type="submit">Ok</Button>
                            <DialogClose asChild>
                                <Button variant="outline">Cancel</Button>
                            </DialogClose>
                        </DialogFooter>
                    </DialogContent>
                </Dialog> */}
            </SidebarGroupAction>
            <SidebarGroupContent>
                <SidebarMenu>
                    {chats
                        .filter(x => x.projectId === null)
                        .map(chat => (
                            <ChatMenuItem
                                key={chat.id}
                                chat={chat}
                            />
                        ))}
                </SidebarMenu>
            </SidebarGroupContent>
        </SidebarGroup>
    );
}

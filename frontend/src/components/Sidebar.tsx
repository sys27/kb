import { PanelLeftClose, Settings, Sparkles } from 'lucide-preact';
import { ChatList } from './ChatList';
import { FolderList } from './FolderList';

export function Sidebar() {
    return (
        <div class="flex flex-col h-full gap-2">
            <div class="flex flex-row gap-1">
                <Sparkles strokeWidth={1.5} />
                <span class="flex-1 font-bold">Knowledge Base</span>
                <PanelLeftClose strokeWidth={1.5} />
            </div>
            <div class="flex-1 overflow-y-scroll">
                <div class="flex flex-col gap-2">
                    <FolderList />
                    <ChatList />
                </div>
            </div>
            <div class="flex gap-1">
                <Settings strokeWidth={1.5} />
                <span>Settings</span>
            </div>
        </div>
    );
}

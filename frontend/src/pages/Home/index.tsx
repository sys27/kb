import { InputPanel } from '../../components/InputPanel';
import { MessageList } from '../../components/MessageList';
import { Sidebar } from '../../components/Sidebar';

export function Home() {
    return (
        <div class="w-screen h-screen flex flex-row divide-x divide-solid divide-neutral-200">
            <div class="w-64 flex-none bg-neutral-100 p-4">
                <Sidebar />
            </div>
            <div class="flex-1 bg-neutral-50 pt-2 pr-8 pb-2 pl-8">
                <div class="flex flex-col h-full gap-4">
                    <div class="flex-1 overflow-y-scroll">
                        <MessageList />
                    </div>
                    <div class="flex-none h-24">
                        <InputPanel />
                    </div>
                </div>
            </div>
        </div>
    );
}

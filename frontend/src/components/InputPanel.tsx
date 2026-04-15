import { Plus, Send } from 'lucide-preact';

export function InputPanel() {
    return (
        <div class="flex flex-row items-center w-full h-full bg-white rounded-xl border border-neutral-300 pl-2 pr-2 gap-2">
            <Plus strokeWidth={1.5} />
            <textarea
                class="h-full flex-1 focus:outline-none focus:ring-0 pt-2 pb-2"
                style="scrollbar-width: none;"
                placeholder="Ask anything"></textarea>
            <Send strokeWidth={1.5} />
        </div>
    );
}

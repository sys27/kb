import {
    Copy,
    Ellipsis,
    FolderClosed,
    FolderPlus,
    MessageCircle,
    Plus,
    Send,
    Settings,
    Sparkles,
} from 'lucide-preact';

export function Home() {
    return (
        <div class="w-screen h-screen flex flex-row divide-x divide-solid divide-neutral-200">
            <div class="w-56 flex-none bg-neutral-100 p-4">
                <div class="flex flex-col h-full gap-2">
                    <div class="flex flex-row gap-1">
                        <Sparkles strokeWidth={1.5} />
                        <span class="font-bold">Knowledge Base</span>
                    </div>
                    <div class="flex-1 overflow-y-scroll">
                        <div class="flex flex-col gap-2">
                            <div class="flex group">
                                <span class="flex-1 font-bold">Folders</span>
                                <FolderPlus
                                    strokeWidth={1.5}
                                    class="size-6 opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                                />
                            </div>
                            <div class="flex gap-1 group pt-1 pl-4 pr-4 pb-1 hover:bg-neutral-200 rounded-lg">
                                <FolderClosed strokeWidth={1.5} />
                                <span class="flex-1">Folder 1</span>
                                <Ellipsis
                                    strokeWidth={1.5}
                                    class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                                />
                            </div>
                            <div class="flex gap-1 group pt-1 pl-4 pr-4 pb-1 hover:bg-neutral-200 rounded-lg">
                                <FolderClosed strokeWidth={1.5} />
                                <span class="flex-1">Folder 2</span>
                                <Ellipsis
                                    strokeWidth={1.5}
                                    class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                                />
                            </div>
                            <div class="flex gap-1 group pt-1 pl-4 pr-4 pb-1 hover:bg-neutral-200 rounded-lg">
                                <FolderClosed strokeWidth={1.5} />
                                <span class="flex-1">Folder 3</span>
                                <Ellipsis
                                    strokeWidth={1.5}
                                    class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                                />
                            </div>
                            <div class="flex group">
                                <span class="flex-1 font-bold">Chats</span>
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                    stroke-width="1.5"
                                    stroke="currentColor"
                                    class="size-6 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                                    <path
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                        d="M12 9v6m3-3H9m12 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z"
                                    />
                                </svg>
                            </div>
                            <div class="flex gap-1 group pt-1 pl-4 pr-4 pb-1 hover:bg-neutral-200 rounded-lg">
                                <MessageCircle strokeWidth={1.5} />
                                <span class="flex-1">Chat 1</span>
                                <Ellipsis
                                    strokeWidth={1.5}
                                    class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                                />
                            </div>
                            <div class="flex gap-1 group pt-1 pl-4 pr-4 pb-1 hover:bg-neutral-200 rounded-lg">
                                <MessageCircle strokeWidth={1.5} />
                                <span class="flex-1">Chat 2</span>
                                <Ellipsis
                                    strokeWidth={1.5}
                                    class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                                />
                            </div>
                            <div class="flex gap-1 group pt-1 pl-4 pr-4 pb-1 hover:bg-neutral-200 rounded-lg">
                                <MessageCircle strokeWidth={1.5} />
                                <span class="flex-1">Chat 3</span>
                                <Ellipsis
                                    strokeWidth={1.5}
                                    class="opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                                />
                            </div>
                        </div>
                    </div>
                    <div class="flex gap-1">
                        <Settings strokeWidth={1.5} />
                        <span>Settings</span>
                    </div>
                </div>
            </div>
            <div class="flex-1 bg-neutral-50 pt-2 pr-8 pb-2 pl-8">
                <div class="flex flex-col h-full gap-4">
                    <div class="flex-1 overflow-y-scroll">
                        <div class="flex flex-col gap-4 items-end">
                            <div class="flex flex-col gap-2 w-xl">
                                <div class="space-y-2 bg-neutral-200 p-4 rounded-xl">
                                    <p>
                                        Lorem ipsum dolor sit amet consectetur adipiscing elit.
                                        Quisque faucibus ex sapien vitae pellentesque sem placerat.
                                        In id cursus mi pretium tellus duis convallis. Tempus leo eu
                                        aenean sed diam urna tempor. Pulvinar vivamus fringilla
                                        lacus nec metus bibendum egestas. Iaculis massa nisl
                                        malesuada lacinia integer nunc posuere. Ut hendrerit semper
                                        vel class aptent taciti sociosqu. Ad litora torquent per
                                        conubia nostra inceptos himenaeos.
                                    </p>
                                </div>
                                <div class="flex flex-row gap-2">
                                    <Copy
                                        size={20}
                                        strokeWidth={1.5}
                                    />
                                </div>
                            </div>
                            <div class="flex flex-col gap-2">
                                <div class="space-y-2">
                                    <p>
                                        Lorem ipsum dolor sit amet consectetur adipiscing elit.
                                        Quisque faucibus ex sapien vitae pellentesque sem placerat.
                                        In id cursus mi pretium tellus duis convallis. Tempus leo eu
                                        aenean sed diam urna tempor. Pulvinar vivamus fringilla
                                        lacus nec metus bibendum egestas. Iaculis massa nisl
                                        malesuada lacinia integer nunc posuere. Ut hendrerit semper
                                        vel class aptent taciti sociosqu. Ad litora torquent per
                                        conubia nostra inceptos himenaeos.
                                    </p>
                                    <p>
                                        Lorem ipsum dolor sit amet consectetur adipiscing elit.
                                        Quisque faucibus ex sapien vitae pellentesque sem placerat.
                                        In id cursus mi pretium tellus duis convallis. Tempus leo eu
                                        aenean sed diam urna tempor. Pulvinar vivamus fringilla
                                        lacus nec metus bibendum egestas. Iaculis massa nisl
                                        malesuada lacinia integer nunc posuere. Ut hendrerit semper
                                        vel class aptent taciti sociosqu. Ad litora torquent per
                                        conubia nostra inceptos himenaeos.
                                    </p>
                                    <p>
                                        Lorem ipsum dolor sit amet consectetur adipiscing elit.
                                        Quisque faucibus ex sapien vitae pellentesque sem placerat.
                                        In id cursus mi pretium tellus duis convallis. Tempus leo eu
                                        aenean sed diam urna tempor. Pulvinar vivamus fringilla
                                        lacus nec metus bibendum egestas. Iaculis massa nisl
                                        malesuada lacinia integer nunc posuere. Ut hendrerit semper
                                        vel class aptent taciti sociosqu. Ad litora torquent per
                                        conubia nostra inceptos himenaeos.
                                    </p>
                                </div>
                                <div class="flex flex-row gap-2">
                                    <Copy
                                        size={20}
                                        strokeWidth={1.5}
                                    />
                                </div>
                            </div>
                            <div class="flex flex-col gap-2 w-xl">
                                <div class="space-y-2 bg-neutral-200 p-4 rounded-xl">
                                    <p>
                                        Lorem ipsum dolor sit amet consectetur adipiscing elit.
                                        Quisque faucibus ex sapien vitae pellentesque sem placerat.
                                        In id cursus mi pretium tellus duis convallis. Tempus leo eu
                                        aenean sed diam urna tempor. Pulvinar vivamus fringilla
                                        lacus nec metus bibendum egestas. Iaculis massa nisl
                                        malesuada lacinia integer nunc posuere. Ut hendrerit semper
                                        vel class aptent taciti sociosqu. Ad litora torquent per
                                        conubia nostra inceptos himenaeos.
                                    </p>
                                </div>
                                <div class="flex flex-row gap-2">
                                    <Copy
                                        size={20}
                                        strokeWidth={1.5}
                                    />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="flex-none h-24">
                        <div class="flex flex-row items-center w-full h-full bg-white rounded-xl border border-neutral-300 pl-2 pr-2 gap-2">
                            <Plus strokeWidth={1.5} />
                            <textarea
                                class="h-full flex-1 focus:outline-none focus:ring-0"
                                style="scrollbar-width: none;"
                                placeholder="Ask anything"></textarea>
                            <Send strokeWidth={1.5} />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

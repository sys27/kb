import { ComponentChildren } from 'preact';
import { useState } from 'preact/hooks';

interface ModalConfig {
    onOk?: (e: Event) => Promise<boolean>;
    onClose?: () => void;
}

interface ModalRendererProps {
    title: string;
    children: ComponentChildren;
}

export function useModal() {
    const [modalState, setModalState] = useState<ModalConfig | null>(null);

    function openModal(config: ModalConfig) {
        setModalState({
            ...config,
            onClose: () => {
                config.onClose?.();
                setModalState(null);
            },
            onOk: async (e: Event) => {
                e.preventDefault();

                let result = await config.onOk?.(e);
                if (result === true) {
                    setModalState(null);
                    return true;
                }

                return false;
            },
        });
    }

    function closeModal() {
        setModalState(null);
    }

    function ModalRenderer({ title, children }: ModalRendererProps) {
        return (
            modalState && (
                <form
                    class="fixed inset-0 flex items-center justify-center bg-black/50"
                    onSubmit={modalState.onOk}>
                    <div class="flex flex-col bg-white rounded-lg shadow-lg relative divide-y-2 divide-solid divide-neutral-200">
                        <div class="flex-none px-2 py-1 bg-neutral-100 rounded-t-lg">{title}</div>
                        <div class="flex-1 p-4">
                            <div class="flex-1 pb-4">{children}</div>
                            <div class="flex-none flex justify-end gap-2">
                                <button
                                    class="px-4 py-1 bg-gray-200 rounded-lg hover:bg-gray-300 transition-colors duration-200"
                                    type="submit">
                                    Ok
                                </button>
                                <button
                                    class="px-4 py-1 bg-gray-200 rounded-lg hover:bg-gray-300 transition-colors duration-200"
                                    type="button"
                                    onClick={modalState.onClose}>
                                    Close
                                </button>
                            </div>
                        </div>
                    </div>
                </form>
            )
        );
    }

    return { openModal, closeModal, ModalRenderer };
}

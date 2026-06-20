import { useMutation } from '@tanstack/react-query';
import { Check, File as FileIcon, FileText, Globe, TriangleAlert } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Button } from '~/components/ui/button';
import { ButtonGroup } from '~/components/ui/button-group';
import {
    Dialog,
    DialogClose,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from '~/components/ui/dialog';
import { Spinner } from '~/components/ui/spinner';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '~/components/ui/table';
import { Tooltip, TooltipContent, TooltipTrigger } from '~/components/ui/tooltip';
import { chatAddWebSite, chatUploadDocument } from '~/services/chat-sources';
import { messagesOptions } from '~/services/messages';
import {
    projectAddWebSite,
    projectDocumentsOptions,
    projectUploadDocument,
} from '~/services/project-documents';
import { AddTextDialog } from './add-text-dialog';
import { AddWebSitesDialog } from './add-web-sites-dialog';

interface AddSourcesDialogProps {
    projectId?: number;
    chatId?: number;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

type Source = {
    id: string;
    state: 'pending' | 'ready' | 'error';
    type: 'document' | 'webSite';
    name: string;
    object: File | string;
    error?: string;
};

async function submitSource({
    projectId,
    chatId,
    source,
}: {
    projectId?: number;
    chatId?: number;
    source: Source;
}): Promise<void> {
    if (projectId) {
        if (source.type === 'webSite' && typeof source.object === 'string') {
            await projectAddWebSite(projectId, source.object);
        }

        if (source.type === 'document' && source.object instanceof File) {
            await projectUploadDocument(projectId, source.object);
        }
    }

    if (chatId) {
        if (source.type === 'webSite' && typeof source.object === 'string') {
            await chatAddWebSite(chatId, source.object);
        }

        if (source.type === 'document' && source.object instanceof File) {
            await chatUploadDocument(chatId, source.object);
        }
    }
}

export function AddSourcesDialog({ projectId, chatId, open, onOpenChange }: AddSourcesDialogProps) {
    let [sources, setSources] = useState<Source[]>([]);
    let [openWebSitesDialog, setOpenWebSitesDialog] = useState(false);
    let [openTextDialog, setOpenTextDialog] = useState(false);

    let sourceMutation = useMutation({
        mutationFn: submitSource,
        onSuccess: (data, variables, onMutateResult, context) => {
            setSources(prev =>
                prev.map(s => (s.id === variables.source.id ? { ...s, state: 'ready' } : s)),
            );

            if (projectId) {
                context.client.invalidateQueries(projectDocumentsOptions(projectId));
            }

            if (chatId) {
                context.client.invalidateQueries(messagesOptions(chatId));
            }
        },
        onError: (error, variables, onMutateResult, context) => {
            setSources(prev =>
                prev.map(s =>
                    s.id === variables.source.id
                        ? {
                              ...s,
                              state: 'error',
                              error:
                                  error instanceof Error ? error.message : 'Failed to add source',
                          }
                        : s,
                ),
            );

            if (projectId) {
                context.client.invalidateQueries(projectDocumentsOptions(projectId));
            }

            if (chatId) {
                context.client.invalidateQueries(messagesOptions(chatId));
            }
        },
    });

    useEffect(() => {
        if (!open) {
            setSources([]);
            setOpenWebSitesDialog(false);
            setOpenTextDialog(false);
        }
    }, [open]);

    async function addSources(newSources: Omit<Source, 'id' | 'state'>[]): Promise<void> {
        let withIds: Source[] = newSources.map(s => ({
            ...s,
            id: crypto.randomUUID(),
            state: 'pending',
        }));

        setSources(prev => [...prev, ...withIds]);

        for (let source of withIds) {
            try {
                await sourceMutation.mutateAsync({ projectId, chatId, source });
            } catch {
                // error is already captured by onError
            }
        }
    }

    function handleFilesPicked(files: FileList | null) {
        if (!files || files.length === 0) {
            return;
        }

        addSources(
            Array.from(files).map(file => ({
                type: 'document' as const,
                name: file.name,
                object: file,
            })),
        );
    }

    let hasPending = sources.some(s => s.state === 'pending');

    return (
        <Dialog
            open={open}
            onOpenChange={onOpenChange}>
            <DialogContent className="min-w-xl">
                <DialogHeader>
                    <DialogTitle>Add Sources</DialogTitle>
                </DialogHeader>

                <div className="flex flex-col gap-2">
                    <ButtonGroup>
                        <Button
                            variant="outline"
                            asChild>
                            <label>
                                <FileIcon />
                                Add Documents
                                <input
                                    type="file"
                                    multiple
                                    className="hidden"
                                    onChange={e => {
                                        handleFilesPicked(e.target.files);
                                        e.target.value = '';
                                    }}
                                />
                            </label>
                        </Button>
                        <Button
                            variant="outline"
                            onClick={() => setOpenWebSitesDialog(true)}>
                            <Globe />
                            Add Web Sites
                        </Button>
                        <Button
                            variant="outline"
                            onClick={() => setOpenTextDialog(true)}>
                            <FileText />
                            Add Text
                        </Button>
                    </ButtonGroup>

                    <Table className="table-fixed">
                        <TableHeader>
                            <TableRow>
                                <TableHead
                                    className="w-8"
                                    align="center"
                                />
                                <TableHead
                                    className="w-12"
                                    align="center">
                                    Type
                                </TableHead>
                                <TableHead>Name</TableHead>
                            </TableRow>
                        </TableHeader>

                        <TableBody>
                            {sources.length === 0 ? (
                                <TableRow>
                                    <TableCell
                                        colSpan={3}
                                        className="text-center">
                                        No Sources
                                    </TableCell>
                                </TableRow>
                            ) : (
                                sources.map(source => (
                                    <TableRow key={source.id}>
                                        <TableCell align="center">
                                            {source.state === 'pending' && <Spinner />}
                                            {source.state === 'ready' && <Check size={16} />}
                                            {source.state === 'error' && (
                                                <Tooltip>
                                                    <TooltipTrigger asChild>
                                                        <TriangleAlert
                                                            size={16}
                                                            className="text-destructive"
                                                        />
                                                    </TooltipTrigger>
                                                    <TooltipContent>{source.error}</TooltipContent>
                                                </Tooltip>
                                            )}
                                        </TableCell>
                                        <TableCell align="center">
                                            {source.type === 'document' && <FileText size={16} />}
                                            {source.type === 'webSite' && <Globe size={16} />}
                                        </TableCell>
                                        <TableCell className="truncate">
                                            {source.type === 'webSite' ? (
                                                <a
                                                    href={source.object as string}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="hover:underline">
                                                    {source.name}
                                                </a>
                                            ) : (
                                                source.name
                                            )}
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </div>

                <DialogFooter>
                    <DialogClose asChild>
                        <Button
                            variant="outline"
                            disabled={hasPending}>
                            Close
                        </Button>
                    </DialogClose>
                </DialogFooter>

                <AddWebSitesDialog
                    open={openWebSitesDialog}
                    onOpenChange={setOpenWebSitesDialog}
                    addWebSites={webSites => {
                        addSources(
                            webSites.map(webSite => ({
                                type: 'webSite' as const,
                                name: webSite,
                                object: webSite,
                            })),
                        );
                    }}
                />

                <AddTextDialog
                    open={openTextDialog}
                    onOpenChange={setOpenTextDialog}
                    addText={text => {
                        let fileName = `${crypto.randomUUID()}.txt`;
                        addSources([
                            {
                                type: 'document' as const,
                                name: fileName,
                                object: new File([text], fileName, { type: 'text/plain' }),
                            },
                        ]);
                    }}
                />
            </DialogContent>
        </Dialog>
    );
}

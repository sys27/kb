import { FolderPlus } from 'lucide-preact';
import { useState } from 'preact/hooks';
import { Folder } from './Folder';

export function FolderList() {
    let [folders, setFolders] = useState([
        { id: 1, name: 'Folder 1' },
        { id: 2, name: 'Folder 2' },
        { id: 3, name: 'Folder 3' },
    ]);

    return (
        <>
            <div class="flex group">
                <span class="flex-1 font-bold">Folders</span>
                <FolderPlus
                    strokeWidth={1.5}
                    class="size-6 opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                />
            </div>
            {folders.map(folder => (
                <Folder
                    key={folder.id}
                    name={folder.name}
                />
            ))}
        </>
    );
}

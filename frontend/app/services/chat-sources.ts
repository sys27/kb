export async function chatUploadDocument(chatId: number, file: File): Promise<void> {
    let formData = new FormData();
    formData.append('file', file);

    let response = await fetch(`/api/chats/${chatId}/sources/upload`, {
        method: 'POST',
        body: formData,
    });
    if (!response.ok) {
        throw new Error('Failed to upload the document');
    }
}

export async function chatAddWebSite(chatId: number, url: string): Promise<void> {
    let response = await fetch(`/api/chats/${chatId}/sources/add-web-site`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ url }),
    });
    if (!response.ok) {
        throw new Error('Failed to add web site');
    }
}


export interface MessageListResponse {
    id: number;
    kind: 'system' | 'user' | 'response';
    text: string;
    timestamp: Date;
}

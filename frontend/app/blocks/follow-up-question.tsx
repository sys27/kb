import { CornerDownRight } from 'lucide-react';
import { Button } from '~/components/ui/button';

export default function FollowUpQuestion({ text, onSubmit }: { text: string; onSubmit: () => void }) {
    return (
        <Button
            variant="ghost"
            size="sm"
            className="justify-start"
            onClick={onSubmit}>
            <CornerDownRight />
            {text}
        </Button>
    );
}

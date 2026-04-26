import { Copy } from 'lucide-react';
import { Button } from '~/components/ui/button';
import {
    Card,
    CardAction,
    CardContent,
    CardFooter,
    CardHeader,
    CardTitle,
} from '~/components/ui/card';
import { Skeleton } from '~/components/ui/skeleton';

export default function MessageSkeletonItem() {
    return (
        <Card>
            <CardHeader>
                <CardTitle>
                    <Skeleton className="h-4 w-40" />
                </CardTitle>
                <CardAction>
                    <Skeleton className="h-4 w-40" />
                </CardAction>
            </CardHeader>

            <CardContent className="whitespace-pre-wrap">
                <Skeleton className="h-40 w-full" />
            </CardContent>

            <CardFooter className="flex flex-row justify-between px-2 py-0">
                <Button
                    variant="ghost"
                    size="icon">
                    <Copy />
                </Button>
                <span className="text-muted-foreground">
                    <Skeleton className="h-4 w-60" />
                </span>
            </CardFooter>
        </Card>
    );
}

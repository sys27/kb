import DOMPurify from 'dompurify';
import hljs from 'highlight.js';
import { Marked, Renderer } from 'marked';
import markedKatex from 'marked-katex-extension';

const renderer = new Renderer();
renderer.code = function ({ text, lang }): string {
    let language = hljs.getLanguage(lang || '') ? lang! : 'plaintext';
    let highlighted = hljs.highlight(text, { language }).value;

    return `<pre class="not-prose whitespace-pre-wrap wrap-break-words overflow-x-auto text-sm"><code class="hljs language-${language}">${highlighted}</code></pre>`;
};

const marked = new Marked({ renderer });
marked.use(
    markedKatex({
        throwOnError: false,
        displayMode: true,
        output: 'mathml',
    }),
);

export function parseMarkdown(text: string): string {
    return DOMPurify.sanitize(marked.parse(text) as string);
}

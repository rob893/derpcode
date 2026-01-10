import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import remarkMath from 'remark-math';
import rehypeHighlight from 'rehype-highlight';
import rehypeKatex from 'rehype-katex';

interface MarkdownRendererProps {
  content: string;
  className?: string;
}

export const MarkdownRenderer = ({ content, className = '' }: MarkdownRendererProps) => {
  return (
    <div className={`prose prose-slate dark:prose-invert max-w-none ${className}`}>
      <ReactMarkdown
        remarkPlugins={[remarkGfm, remarkMath]}
        rehypePlugins={[rehypeKatex, rehypeHighlight]}
        components={{
          // Customize specific markdown elements to work better with Tailwind/HeroUI
          h1: ({ ...props }) => <h1 className="text-2xl font-bold mb-4 text-foreground" {...props} />,
          h2: ({ ...props }) => <h2 className="text-xl font-semibold mb-3 text-foreground" {...props} />,
          h3: ({ ...props }) => <h3 className="text-lg font-semibold mb-2 text-foreground" {...props} />,
          h4: ({ ...props }) => <h4 className="text-base font-semibold mb-2 text-foreground" {...props} />,
          h5: ({ ...props }) => <h5 className="text-sm font-semibold mb-2 text-foreground" {...props} />,
          h6: ({ ...props }) => <h6 className="text-xs font-semibold mb-2 text-foreground" {...props} />,
          p: ({ ...props }) => <p className="text-default-600 leading-relaxed mb-4" {...props} />,
          ul: ({ ...props }) => <ul className="list-disc list-inside mb-4 text-default-600 space-y-1" {...props} />,
          ol: ({ ...props }) => <ol className="list-decimal list-inside mb-4 text-default-600 space-y-1" {...props} />,
          li: ({ ...props }) => <li className="text-default-600" {...props} />,
          blockquote: ({ ...props }) => (
            <blockquote
              className="border-l-4 border-primary pl-4 py-2 my-4 bg-default-50 dark:bg-default-100/50 italic text-default-700"
              {...props}
            />
          ),
          code: ({ className, children, ...props }) => {
            const isInline = !className;
            if (isInline) {
              return (
                <code
                  className="bg-default-100 dark:bg-default-200/20 px-1.5 py-0.5 rounded-sm text-sm font-mono text-primary"
                  {...props}
                >
                  {children}
                </code>
              );
            }
            return (
              <code
                className={`block bg-default-100 dark:bg-default-200/10 p-4 rounded-lg overflow-x-auto text-sm font-mono ${className || ''}`}
                {...props}
              >
                {children}
              </code>
            );
          },
          pre: ({ ...props }) => (
            <pre className="bg-default-100 dark:bg-default-200/10 p-4 rounded-lg overflow-x-auto mb-4" {...props} />
          ),
          table: ({ ...props }) => (
            <div className="overflow-x-auto mb-4">
              <table className="min-w-full border-collapse border border-divider" {...props} />
            </div>
          ),
          th: ({ ...props }) => (
            <th
              className="border border-divider px-4 py-2 bg-default-100 text-left font-semibold text-foreground"
              {...props}
            />
          ),
          td: ({ ...props }) => <td className="border border-divider px-4 py-2 text-default-600" {...props} />,
          a: ({ ...props }) => (
            <a
              className="text-primary hover:text-primary-600 underline transition-colors"
              target="_blank"
              rel="noopener noreferrer"
              {...props}
            />
          ),
          strong: ({ ...props }) => <strong className="font-semibold text-foreground" {...props} />,
          em: ({ ...props }) => <em className="italic text-default-700" {...props} />,
          hr: ({ ...props }) => <hr className="border-divider my-6" {...props} />
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
};

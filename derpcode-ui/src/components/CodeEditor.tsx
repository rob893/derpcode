import { useState, useRef, useCallback, useEffect } from 'react';
import Editor from '@monaco-editor/react';
import { Language } from '../types/models';

interface CodeEditorProps {
  language: Language;
  code: string;
  uiTemplate: string;
  onChange(value: string | undefined): void;
  flamesEnabled?: boolean;
  readOnly?: boolean;
}

interface FlameParticle {
  id: string;
  x: number;
  y: number;
  opacity: number;
  size: number;
  velocity: { x: number; y: number };
}

export const CodeEditor = ({
  language,
  code,
  onChange,
  uiTemplate,
  flamesEnabled = true,
  readOnly = false
}: CodeEditorProps) => {
  const [flames, setFlames] = useState<FlameParticle[]>([]);
  const editorRef = useRef<any>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const lastFlameTimeRef = useRef<number>(0);

  const getMonacoLanguage = (lang: Language) => {
    switch (lang) {
      case Language.CSharp:
        return 'csharp';
      case Language.JavaScript:
        return 'javascript';
      case Language.TypeScript:
        return 'typescript';
      case Language.Rust:
        return 'rust';
      case Language.Python:
        return 'python';
      case Language.Java:
        return 'java';
      default:
        return 'javascript';
    }
  };

  const createFlameParticles = useCallback((x: number, y: number) => {
    // Increase particle count for more noticeable effect (2-3 particles)
    const particleCount = Math.random() > 0.5 ? 1 : 2;
    const newFlames: FlameParticle[] = [];

    for (let i = 0; i < particleCount; i++) {
      newFlames.push({
        id: `flame-${Date.now()}-${i}`,
        x: x + (Math.random() - 0.5) * 12, // Wider spread for more visibility
        y: y + (Math.random() - 0.5) * 8,
        opacity: 1,
        size: Math.random() * 10 + 5, // Larger flames (8-18px)
        velocity: {
          x: (Math.random() - 0.5) * 1.5,
          y: -(Math.random() * 3 + 1) // Faster upward movement
        }
      });
    }

    setFlames(prev => [...prev, ...newFlames]);

    // Keep flames longer for better visibility
    setTimeout(() => {
      setFlames(prev => prev.filter(flame => !newFlames.some(newFlame => newFlame.id === flame.id)));
    }, 1000);
  }, []);

  const handleEditorChange = useCallback(
    (value: string | undefined) => {
      onChange(value);

      // Only create flames if enabled
      if (!flamesEnabled) {
        return;
      }

      // Throttle flame creation but allow more frequent spawning (max 1 flame every 100ms)
      const now = Date.now();
      if (now - lastFlameTimeRef.current < 200) {
        return;
      }
      lastFlameTimeRef.current = now;

      // Get cursor position and create flames
      if (editorRef.current && containerRef.current) {
        try {
          const editor = editorRef.current;
          const position = editor.getPosition();

          if (position) {
            // Get the precise cursor coordinates
            const domNode = editor.getDomNode();

            // Use Monaco's coordinate transformation to get precise cursor position
            const coords = editor.getScrolledVisiblePosition(position);

            if (coords && domNode) {
              const editorRect = domNode.getBoundingClientRect();
              const containerRect = containerRef.current.getBoundingClientRect();

              // Calculate precise position relative to container
              const relativeX = coords.left + (editorRect.left - containerRect.left);
              const relativeY = coords.top + (editorRect.top - containerRect.top);

              // Only create flames if coordinates are valid
              if (relativeX > 0 && relativeY > 0) {
                createFlameParticles(relativeX, relativeY);
              }
            }
          }
        } catch {
          // Silently fail if cursor position detection fails
          // No fallback to avoid performance issues
        }
      }
    },
    [onChange, createFlameParticles, flamesEnabled]
  );

  // Animate flame particles (optimized)
  useEffect(() => {
    if (flames.length === 0) return;

    const animationFrame = requestAnimationFrame(() => {
      setFlames(prev =>
        prev
          .map(flame => ({
            ...flame,
            x: flame.x + flame.velocity.x,
            y: flame.y + flame.velocity.y,
            opacity: flame.opacity - 0.025, // Slower fade for longer visibility
            size: flame.size * 0.98 // Slower shrink
          }))
          .filter(flame => flame.opacity > 0.1)
      ); // Keep longer
    });

    return () => cancelAnimationFrame(animationFrame);
  }, [flames]);

  return (
    <div ref={containerRef} className="relative rounded-lg overflow-hidden border border-divider bg-content1">
      <Editor
        height="70vh"
        language={getMonacoLanguage(language)}
        value={code || uiTemplate}
        onChange={handleEditorChange}
        onMount={editor => {
          editorRef.current = editor;
        }}
        theme="vs-dark"
        options={{
          minimap: { enabled: false },
          fontSize: 14,
          lineNumbers: 'on',
          roundedSelection: false,
          scrollBeyondLastLine: false,
          automaticLayout: true,
          readOnly: readOnly
        }}
      />

      {/* Flame particles overlay */}
      {flamesEnabled && (
        <div className="absolute inset-0 pointer-events-none z-10">
          {flames.map(flame => (
            <div
              key={flame.id}
              className="absolute transition-all duration-100 ease-out"
              style={{
                left: `${flame.x}px`,
                top: `${flame.y}px`,
                width: `${flame.size}px`,
                height: `${flame.size}px`,
                opacity: flame.opacity,
                transform: 'translate(-50%, -50%)'
              }}
            >
              {/* Enhanced flame emoji with brighter glow effect */}
              <div
                className="text-red-500 select-none"
                style={{
                  fontSize: `${flame.size}px`,
                  filter: `drop-shadow(0 0 ${flame.size * 0.3}px rgba(255, 69, 0, 0.8)) drop-shadow(0 0 ${flame.size * 0.5}px rgba(255, 165, 0, 0.4))`,
                  textShadow: `0 0 ${flame.size * 0.3}px rgba(255, 69, 0, 0.8), 0 0 ${flame.size * 0.6}px rgba(255, 140, 0, 0.6)`
                }}
              >
                🔥
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

import { useState, useRef, useCallback, useEffect } from 'react';
import type { ReactNode } from 'react';

interface ResizableSplitterProps {
  leftPanel: ReactNode;
  rightPanel: ReactNode;
  defaultLeftWidth?: number;
  minLeftWidth?: number;
  maxLeftWidth?: number;
  className?: string;
}

export const ResizableSplitter = ({
  leftPanel,
  rightPanel,
  defaultLeftWidth = 50, // Default to 1/2 width (matches current lg:col-span-1)
  minLeftWidth = 20,
  maxLeftWidth = 80,
  className = ''
}: ResizableSplitterProps) => {
  const [leftWidth, setLeftWidth] = useState(defaultLeftWidth);
  const [isDragging, setIsDragging] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const dragStartX = useRef<number>(0);
  const dragStartWidth = useRef<number>(0);

  const handleMouseDown = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      setIsDragging(true);
      dragStartX.current = e.clientX;
      dragStartWidth.current = leftWidth;
    },
    [leftWidth]
  );

  const handleMouseMove = useCallback(
    (e: MouseEvent) => {
      if (!isDragging || !containerRef.current) return;

      const containerRect = containerRef.current.getBoundingClientRect();
      const deltaX = e.clientX - dragStartX.current;
      const deltaPercentage = (deltaX / containerRect.width) * 100;
      const newWidth = Math.max(minLeftWidth, Math.min(maxLeftWidth, dragStartWidth.current + deltaPercentage));

      setLeftWidth(newWidth);
    },
    [isDragging, minLeftWidth, maxLeftWidth]
  );

  const handleMouseUp = useCallback(() => {
    setIsDragging(false);
  }, []);

  // Add event listeners for mouse move and up
  useEffect(() => {
    if (isDragging) {
      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleMouseUp);
      document.body.style.cursor = 'col-resize';
      document.body.style.userSelect = 'none';

      return () => {
        document.removeEventListener('mousemove', handleMouseMove);
        document.removeEventListener('mouseup', handleMouseUp);
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
      };
    }
  }, [isDragging, handleMouseMove, handleMouseUp]);

  return (
    <div ref={containerRef} className={`flex h-full w-full ${className}`}>
      {/* Left Panel */}
      <div style={{ width: `${leftWidth}%` }} className="shrink-0 overflow-hidden h-full">
        {leftPanel}
      </div>

      {/* Resizable Divider */}
      <div
        className={`
          w-1 bg-divider hover:bg-primary/50 cursor-col-resize shrink-0 relative
          transition-colors duration-200
          ${isDragging ? 'bg-primary/70' : ''}
        `}
        onMouseDown={handleMouseDown}
      >
        {/* Visual indicator when hovering/dragging */}
        <div
          className={`
          absolute inset-y-0 -inset-x-1 
          ${isDragging ? 'bg-primary/20' : 'hover:bg-primary/10'}
          transition-colors duration-200
        `}
        />
      </div>

      {/* Right Panel */}
      <div style={{ width: `${100 - leftWidth}%` }} className="shrink-0 overflow-hidden h-full">
        {rightPanel}
      </div>
    </div>
  );
};

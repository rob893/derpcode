import { Chip } from '@heroui/react';

export function WarningBanner() {
  return (
    <div className="bg-warning-50 border-b border-warning-200 py-3 px-4">
      <div className="max-w-7xl mx-auto flex items-center justify-center gap-3">
        <Chip color="warning" variant="flat" size="sm" className="flex-shrink-0">
          ⚠️ DEVELOPMENT
        </Chip>
        <div className="text-warning-800 text-sm font-medium text-center">
          <span className="hidden sm:inline">This app is under active development. </span>
          <span className="font-semibold">Data loss is likely!</span>
          <span className="hidden md:inline"> Your progress and submissions may be lost at any time.</span>
        </div>
      </div>
    </div>
  );
}

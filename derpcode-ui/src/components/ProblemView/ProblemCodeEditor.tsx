import { useState, useEffect } from 'react';
import {
  Card,
  CardBody,
  CardHeader,
  Button,
  Select,
  SelectItem,
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  useDisclosure,
  Switch,
  Tooltip
} from '@heroui/react';
import {
  Cog6ToothIcon,
  ExclamationTriangleIcon,
  ArrowsPointingOutIcon,
  ArrowsPointingInIcon,
  ArrowPathIcon,
  PlayIcon
} from '@heroicons/react/24/outline';
import { PlayIcon as PlayIconSolid } from '@heroicons/react/24/solid';
import { Language } from '../../types/models';
import type { Problem, ProblemSubmission } from '../../types/models';
import type { User } from '../../types/auth';
import { CodeEditor } from '../CodeEditor';
import { loadCodeWithPriority } from '../../utils/localStorageUtils';
import { getLanguageLabel } from '../../utils/utilities';
import { useUserPreferences } from '../../hooks/api';

interface ProblemCodeEditorProps {
  problem: Problem;
  user: User | null;
  selectedLanguage: Language | undefined;
  code: string;
  onLanguageChange: (language: Language) => void;
  onCodeChange: (code: string) => void;
  onSubmit: () => void;
  onRun: () => void;
  onReset: () => void;
  isSubmitting: boolean;
  isRunning: boolean;
  selectedSubmission?: ProblemSubmission | null;
  onReturnToWorkingCode?: () => void;
}

export const ProblemCodeEditor = ({
  problem,
  user,
  selectedLanguage,
  code,
  onLanguageChange,
  onCodeChange,
  onSubmit,
  onRun,
  onReset,
  isSubmitting,
  isRunning,
  selectedSubmission,
  onReturnToWorkingCode
}: ProblemCodeEditorProps) => {
  const { data: userPreferences } = useUserPreferences(user?.id);
  const flamesEnabledPref = userPreferences?.preferences.editorPreference.enableFlameEffects ?? true;
  const [flamesEnabled, setFlamesEnabled] = useState(flamesEnabledPref);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const { isOpen: isSettingsOpen, onOpen: onSettingsOpen, onOpenChange: onSettingsOpenChange } = useDisclosure();

  const handleLanguageChange = (newLanguage: Language) => {
    onLanguageChange(newLanguage);

    const selectedDriver = problem.drivers.find(driver => driver.language === newLanguage);
    if (selectedDriver) {
      // Try to restore saved code for the new language
      const { code: savedCode } = loadCodeWithPriority(user?.id || null, problem.id, newLanguage);

      // Use saved code if available, otherwise use the template
      onCodeChange(savedCode || selectedDriver.uiTemplate);
    }
  };

  const toggleFullscreen = () => {
    setIsFullscreen(!isFullscreen);
  };

  // Handle escape key to exit fullscreen
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && isFullscreen) {
        setIsFullscreen(false);
      }
    };

    if (isFullscreen) {
      document.addEventListener('keydown', handleKeyDown);
      return () => {
        document.removeEventListener('keydown', handleKeyDown);
      };
    }
  }, [isFullscreen]);

  return (
    <>
      {/* Fullscreen Overlay - positioned below AppHeader and WarningBanner */}
      {isFullscreen && (
        <div className="fixed top-[114px] left-0 right-0 bottom-0 z-40 bg-background flex flex-col p-8 pt-8">
          {/* Fullscreen Code Editor Card */}
          <div className="flex-1 bg-content1 rounded-lg border border-divider shadow-lg flex flex-col">
            {/* Fullscreen Header */}
            <div className="flex items-center justify-between p-4 bg-content1 rounded-t-lg">
              <div className="flex items-center gap-3 min-w-0 flex-1">
                <h3 className="text-xl font-semibold whitespace-nowrap">
                  {selectedSubmission ? 'Submission Code' : 'Code Editor'}
                </h3>
                {!selectedSubmission && (
                  <Select
                    label="Language"
                    selectedKeys={selectedLanguage ? [selectedLanguage] : []}
                    onSelectionChange={keys => {
                      const newLanguage = Array.from(keys)[0] as Language;
                      handleLanguageChange(newLanguage);
                    }}
                    className="w-32 shrink-0"
                    size="sm"
                  >
                    {problem.drivers.map(driver => (
                      <SelectItem key={driver.language}>{getLanguageLabel(driver.language)}</SelectItem>
                    ))}
                  </Select>
                )}
                {selectedSubmission && (
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-default-500">Language:</span>
                    <span className="text-sm font-medium">{selectedSubmission.language}</span>
                    <span className="text-sm text-default-500">|</span>
                    <span className="text-sm text-default-500">Status:</span>
                    <div className="flex items-center gap-1">
                      <span
                        className={`text-sm font-medium ${selectedSubmission.pass ? 'text-success' : 'text-danger'}`}
                      >
                        {selectedSubmission.pass ? 'Accepted' : 'Failed'}
                      </span>
                      {!selectedSubmission.pass && selectedSubmission.errorMessage && (
                        <ExclamationTriangleIcon className="h-4 w-4 text-danger" />
                      )}
                    </div>
                  </div>
                )}
              </div>

              {/* Center Action Buttons - only show for non-submission mode */}
              {!selectedSubmission && (
                <div className="flex items-center gap-2 absolute left-1/2 transform -translate-x-1/2">
                  <Tooltip content="Reset Code" placement="bottom">
                    <Button
                      isIconOnly
                      color="warning"
                      variant="bordered"
                      size="md"
                      onPress={onReset}
                      isDisabled={!selectedLanguage}
                      aria-label="Reset Code"
                      className="shrink-0"
                    >
                      <ArrowPathIcon className="h-5 w-5" />
                    </Button>
                  </Tooltip>
                  <Tooltip content={isRunning ? 'Running...' : 'Run Solution'} placement="bottom">
                    <Button
                      isIconOnly
                      color="secondary"
                      variant="bordered"
                      size="md"
                      isLoading={isRunning}
                      isDisabled={!code.trim() || !selectedLanguage || isSubmitting}
                      onPress={onRun}
                      aria-label={isRunning ? 'Running...' : 'Run Solution'}
                      className="shrink-0"
                    >
                      <PlayIcon className="h-5 w-5" />
                    </Button>
                  </Tooltip>
                  <Tooltip content={isSubmitting ? 'Submitting...' : 'Submit Solution'} placement="bottom">
                    <Button
                      isIconOnly
                      color="primary"
                      size="md"
                      isLoading={isSubmitting}
                      isDisabled={!code.trim() || !selectedLanguage || isRunning}
                      onPress={onSubmit}
                      aria-label={isSubmitting ? 'Submitting...' : 'Submit Solution'}
                      className="shrink-0"
                    >
                      <PlayIconSolid className="h-5 w-5" />
                    </Button>
                  </Tooltip>
                </div>
              )}

              <div className="flex items-center gap-2 shrink-0">
                {selectedSubmission && onReturnToWorkingCode && (
                  <Button
                    color="primary"
                    variant="bordered"
                    size="sm"
                    onPress={onReturnToWorkingCode}
                    className="font-semibold"
                  >
                    Return to Editor
                  </Button>
                )}
                <Tooltip content="Exit Fullscreen" placement="bottom">
                  <Button
                    isIconOnly
                    variant="light"
                    size="md"
                    onPress={toggleFullscreen}
                    aria-label="Exit fullscreen"
                    className="shrink-0"
                  >
                    <ArrowsPointingInIcon className="h-5 w-5" />
                  </Button>
                </Tooltip>
                <Tooltip content="Settings" placement="bottom">
                  <Button
                    isIconOnly
                    variant="light"
                    size="md"
                    onPress={onSettingsOpen}
                    aria-label="Settings"
                    className="shrink-0"
                  >
                    <Cog6ToothIcon className="h-5 w-5" />
                  </Button>
                </Tooltip>
              </div>
            </div>

            {/* Fullscreen Error Message */}
            {selectedSubmission && !selectedSubmission.pass && selectedSubmission.errorMessage && (
              <div className="px-6 pb-3">
                <div className="bg-danger/10 border border-danger/20 rounded-lg p-3">
                  <div className="flex items-start gap-2">
                    <ExclamationTriangleIcon className="h-5 w-5 text-danger shrink-0 mt-0.5" />
                    <div>
                      <h4 className="text-sm font-medium text-danger mb-1">Execution Error</h4>
                      <p className="text-sm text-danger/80">{selectedSubmission.errorMessage}</p>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Fullscreen Code Editor */}
            <div className="flex-1 pt-0 px-6 pb-6 overflow-hidden">
              <div className="h-full space-y-4">
                <CodeEditor
                  language={selectedLanguage || Language.JavaScript}
                  code={code}
                  onChange={selectedSubmission ? () => {} : value => onCodeChange(value ?? '')}
                  uiTemplate={problem.drivers.find(d => d.language === selectedLanguage)?.uiTemplate ?? ''}
                  flamesEnabled={flamesEnabled && !selectedSubmission}
                  readOnly={!!selectedSubmission}
                  editorHeight="calc(100vh - 300px)"
                />
              </div>
            </div>
          </div>
        </div>
      )}{' '}
      {/* Normal Card View */}
      <Card>
        <CardHeader className="pb-3">
          <div className="flex items-center justify-between w-full">
            <div className="flex items-center gap-3 min-w-0 flex-1">
              <h3 className="text-xl font-semibold whitespace-nowrap">
                {selectedSubmission ? 'Submission Code' : 'Code Editor'}
              </h3>
              {!selectedSubmission && (
                <Select
                  label="Language"
                  selectedKeys={selectedLanguage ? [selectedLanguage] : []}
                  onSelectionChange={keys => {
                    const newLanguage = Array.from(keys)[0] as Language;
                    handleLanguageChange(newLanguage);
                  }}
                  className="w-32 shrink-0"
                  size="sm"
                >
                  {problem.drivers.map(driver => (
                    <SelectItem key={driver.language}>{getLanguageLabel(driver.language)}</SelectItem>
                  ))}
                </Select>
              )}
              {selectedSubmission && (
                <div className="flex items-center gap-2">
                  <span className="text-sm text-default-500">Language:</span>
                  <span className="text-sm font-medium">{selectedSubmission.language}</span>
                  <span className="text-sm text-default-500">|</span>
                  <span className="text-sm text-default-500">Status:</span>
                  <div className="flex items-center gap-1">
                    <span className={`text-sm font-medium ${selectedSubmission.pass ? 'text-success' : 'text-danger'}`}>
                      {selectedSubmission.pass ? 'Accepted' : 'Failed'}
                    </span>
                    {!selectedSubmission.pass && selectedSubmission.errorMessage && (
                      <ExclamationTriangleIcon className="h-4 w-4 text-danger" />
                    )}
                  </div>
                </div>
              )}
            </div>

            {/* Center Action Buttons - only show for non-submission mode */}
            {!selectedSubmission && (
              <div className="flex items-center gap-2 absolute left-1/2 transform -translate-x-1/2">
                <Tooltip content="Reset Code" placement="bottom">
                  <Button
                    isIconOnly
                    color="warning"
                    variant="bordered"
                    size="md"
                    onPress={onReset}
                    isDisabled={!selectedLanguage}
                    aria-label="Reset Code"
                    className="shrink-0"
                  >
                    <ArrowPathIcon className="h-5 w-5" />
                  </Button>
                </Tooltip>
                <Tooltip content={isRunning ? 'Running...' : 'Run Solution'} placement="bottom">
                  <Button
                    isIconOnly
                    color="secondary"
                    variant="bordered"
                    size="md"
                    isLoading={isRunning}
                    isDisabled={!code.trim() || !selectedLanguage || isSubmitting}
                    onPress={onRun}
                    aria-label={isRunning ? 'Running...' : 'Run Solution'}
                    className="shrink-0"
                  >
                    <PlayIcon className="h-5 w-5" />
                  </Button>
                </Tooltip>
                <Tooltip content={isSubmitting ? 'Submitting...' : 'Submit Solution'} placement="bottom">
                  <Button
                    isIconOnly
                    color="primary"
                    size="md"
                    isLoading={isSubmitting}
                    isDisabled={!code.trim() || !selectedLanguage || isRunning}
                    onPress={onSubmit}
                    aria-label={isSubmitting ? 'Submitting...' : 'Submit Solution'}
                    className="shrink-0"
                  >
                    <PlayIconSolid className="h-5 w-5" />
                  </Button>
                </Tooltip>
              </div>
            )}

            <div className="flex items-center gap-2 shrink-0">
              {selectedSubmission && onReturnToWorkingCode && (
                <Button
                  color="primary"
                  variant="bordered"
                  size="sm"
                  onPress={onReturnToWorkingCode}
                  className="font-semibold"
                >
                  Return to Editor
                </Button>
              )}
              <Tooltip content={isFullscreen ? 'Exit Fullscreen' : 'Enter Fullscreen'} placement="bottom">
                <Button
                  isIconOnly
                  variant="light"
                  size="md"
                  onPress={toggleFullscreen}
                  aria-label={isFullscreen ? 'Exit fullscreen' : 'Enter fullscreen'}
                  className="shrink-0"
                >
                  {isFullscreen ? (
                    <ArrowsPointingInIcon className="h-5 w-5" />
                  ) : (
                    <ArrowsPointingOutIcon className="h-5 w-5" />
                  )}
                </Button>
              </Tooltip>
              <Tooltip content="Settings" placement="bottom">
                <Button
                  isIconOnly
                  variant="light"
                  size="md"
                  onPress={onSettingsOpen}
                  aria-label="Settings"
                  className="shrink-0"
                >
                  <Cog6ToothIcon className="h-5 w-5" />
                </Button>
              </Tooltip>
            </div>
          </div>
        </CardHeader>
        {selectedSubmission && !selectedSubmission.pass && selectedSubmission.errorMessage && (
          <div className="px-6 pb-3">
            <div className="bg-danger/10 border border-danger/20 rounded-lg p-3">
              <div className="flex items-start gap-2">
                <ExclamationTriangleIcon className="h-5 w-5 text-danger shrink-0 mt-0.5" />
                <div>
                  <h4 className="text-sm font-medium text-danger mb-1">Execution Error</h4>
                  <p className="text-sm text-danger/80">{selectedSubmission.errorMessage}</p>
                </div>
              </div>
            </div>
          </div>
        )}
        <CardBody className="pt-0">
          <div className="space-y-4">
            <CodeEditor
              language={selectedLanguage || Language.JavaScript}
              code={code}
              onChange={selectedSubmission ? () => {} : value => onCodeChange(value ?? '')}
              uiTemplate={problem.drivers.find(d => d.language === selectedLanguage)?.uiTemplate ?? ''}
              flamesEnabled={flamesEnabled && !selectedSubmission}
              readOnly={!!selectedSubmission}
              editorHeight="79vh"
            />
          </div>
        </CardBody>
      </Card>
      {/* Settings Modal */}
      <Modal isOpen={isSettingsOpen} onOpenChange={onSettingsOpenChange} placement="center">
        <ModalContent>
          {onClose => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                <h2 className="text-xl font-bold text-foreground">Editor Settings</h2>
              </ModalHeader>
              <ModalBody>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="text-medium font-semibold">Flame Effects</h3>
                      <p className="text-small text-default-500">Show fire animations when typing</p>
                    </div>
                    <Switch isSelected={flamesEnabled} onValueChange={setFlamesEnabled} color="primary" />
                  </div>
                </div>
              </ModalBody>
              <ModalFooter>
                <Button color="primary" onPress={onClose}>
                  Done
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>
    </>
  );
};

import { useState } from 'react';
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
  Switch
} from '@heroui/react';
import { Cog6ToothIcon, ExclamationTriangleIcon } from '@heroicons/react/24/outline';
import { Language } from '../../types/models';
import type { Problem, ProblemSubmission } from '../../types/models';
import { CodeEditor } from '../CodeEditor';
import { loadCodeWithPriority } from '../../utils/localStorageUtils';

interface ProblemCodeEditorProps {
  problem: Problem;
  user: any;
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
  const [flamesEnabled, setFlamesEnabled] = useState(true);
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

  return (
    <>
      <Card>
        <CardHeader className="pb-3">
          <div className="flex justify-between items-center w-full">
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
                  className="w-32 flex-shrink-0"
                  size="sm"
                >
                  {problem.drivers.map(driver => (
                    <SelectItem key={driver.language}>{driver.language}</SelectItem>
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
            <div className="flex items-center gap-2 flex-shrink-0">
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
              <Button
                isIconOnly
                variant="light"
                size="md"
                onPress={onSettingsOpen}
                aria-label="Settings"
                className="flex-shrink-0"
              >
                <Cog6ToothIcon className="h-5 w-5" />
              </Button>
            </div>
          </div>
        </CardHeader>
        {selectedSubmission && !selectedSubmission.pass && selectedSubmission.errorMessage && (
          <div className="px-6 pb-3">
            <div className="bg-danger/10 border border-danger/20 rounded-lg p-3">
              <div className="flex items-start gap-2">
                <ExclamationTriangleIcon className="h-5 w-5 text-danger flex-shrink-0 mt-0.5" />
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
            />

            {!selectedSubmission && (
              <div className="flex justify-end gap-3">
                <Button
                  color="warning"
                  variant="bordered"
                  size="lg"
                  onPress={onReset}
                  isDisabled={!selectedLanguage}
                  className="font-semibold"
                >
                  Reset Code
                </Button>
                <Button
                  color="secondary"
                  variant="bordered"
                  size="lg"
                  isLoading={isRunning}
                  isDisabled={!code.trim() || !selectedLanguage}
                  onPress={onRun}
                  className="font-semibold"
                >
                  {isRunning ? 'Running...' : 'Run Solution'}
                </Button>
                <Button
                  color="primary"
                  size="lg"
                  isLoading={isSubmitting}
                  isDisabled={!code.trim() || !selectedLanguage}
                  onPress={onSubmit}
                  className="font-semibold"
                >
                  {isSubmitting ? 'Submitting...' : 'Submit Solution'}
                </Button>
              </div>
            )}
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

import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import {
  Card,
  CardBody,
  CardHeader,
  Button,
  Chip,
  Select,
  SelectItem,
  Spinner,
  Divider,
  Code as CodeBlock,
  Modal,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  useDisclosure,
  Switch
} from '@heroui/react';
import { ArrowLeftIcon, EyeIcon, EyeSlashIcon, Cog6ToothIcon } from '@heroicons/react/24/outline';
import { Language, ProblemDifficulty } from '../types/models';
import type { SubmissionResult } from '../types/models';
import { CodeEditor } from './CodeEditor';
import { ApiErrorDisplay } from './ApiErrorDisplay';
import { useProblem, useSubmitSolution } from '../hooks/api';
import { useAuth } from '../hooks/useAuth';
import { useAutoSave } from '../hooks/useAutoSave';
import { loadCodeWithPriority, cleanupOldAutoSaveData } from '../utils/localStorageUtils';

export const ProblemView = () => {
  const { id } = useParams<{ id: string }>();
  const { user, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const { data: problem, isLoading, error } = useProblem(Number(id!));
  const submitSolution = useSubmitSolution(problem?.id || 0);
  const { isOpen, onOpen, onOpenChange } = useDisclosure();
  const { isOpen: isSettingsOpen, onOpen: onSettingsOpen, onOpenChange: onSettingsOpenChange } = useDisclosure();

  const [selectedLanguage, setSelectedLanguage] = useState<Language | undefined>(undefined);
  const [code, setCode] = useState('');
  const [result, setResult] = useState<SubmissionResult | null>(null);
  const [submissionError, setSubmissionError] = useState<Error | null>(null);
  const [showHints, setShowHints] = useState(false);
  const [flamesEnabled, setFlamesEnabled] = useState(true);

  // Auto-save functionality
  useAutoSave(
    code,
    user?.id || null,
    problem?.id || 0,
    selectedLanguage || Language.JavaScript,
    3000 // 3 seconds delay
  );

  // Set initial language and template when problem data is loaded
  useEffect(() => {
    if (problem && problem.drivers.length > 0) {
      // Default to JavaScript if available, otherwise use the first driver
      const initialDriver = problem.drivers.find(d => d.language === Language.JavaScript) || problem.drivers[0];
      setSelectedLanguage(initialDriver.language);

      // Try to restore saved code with priority logic
      const { code: savedCode } = loadCodeWithPriority(user?.id || null, problem.id, initialDriver.language);

      // Use saved code if available, otherwise use the template
      setCode(savedCode || initialDriver.uiTemplate);
    }
  }, [problem, user?.id]);

  // Cleanup old auto-save data on component mount
  useEffect(() => {
    cleanupOldAutoSaveData(30); // Keep data for 30 days
  }, []);

  const getDifficultyColor = (difficulty: ProblemDifficulty) => {
    switch (difficulty) {
      case ProblemDifficulty.VeryEasy:
      case ProblemDifficulty.Easy:
        return 'success';
      case ProblemDifficulty.Medium:
        return 'warning';
      case ProblemDifficulty.Hard:
      case ProblemDifficulty.VeryHard:
        return 'danger';
      default:
        return 'default';
    }
  };

  const getDifficultyLabel = (difficulty: ProblemDifficulty): string => {
    switch (difficulty) {
      case ProblemDifficulty.VeryEasy:
        return 'Very Easy';
      case ProblemDifficulty.Easy:
        return 'Easy';
      case ProblemDifficulty.Medium:
        return 'Medium';
      case ProblemDifficulty.Hard:
        return 'Hard';
      case ProblemDifficulty.VeryHard:
        return 'Very Hard';
      default:
        return 'Unknown';
    }
  };

  const handleSubmit = async () => {
    if (!problem || !selectedLanguage) return;

    // Check if user is authenticated
    if (!isAuthenticated || !user) {
      onOpen(); // Show login modal
      return;
    }

    try {
      setSubmissionError(null); // Clear any previous errors
      const submissionResult = await submitSolution.mutateAsync({
        userCode: code,
        language: selectedLanguage,
        userId: user.id || 0
      });
      setResult(submissionResult);
    } catch (error) {
      console.error('Submission error:', error);
      setSubmissionError(error as Error);
      setResult(null); // Clear any previous results
    }
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <Spinner size="lg" color="primary" label="Loading problem..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <ApiErrorDisplay error={error} title="Failed to load problem" className="max-w-md mx-auto" showDetails={true} />
      </div>
    );
  }

  if (!problem) {
    return (
      <Card className="max-w-md mx-auto">
        <CardBody className="text-center py-8">
          <p className="text-warning text-lg">Problem not found</p>
        </CardBody>
      </Card>
    );
  }

  const formatValue = (value: any): string => {
    return JSON.stringify(value, null, 2);
  };

  return (
    <div className="space-y-6">
      <Button
        variant="ghost"
        color="primary"
        startContent={<ArrowLeftIcon className="h-4 w-4" />}
        onPress={() => navigate('/problems')}
        className="mb-4"
      >
        Back to Problems
      </Button>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-1 space-y-6">
          <Card>
            <CardHeader className="pb-3">
              <div className="flex justify-between items-start w-full">
                <h2 className="text-2xl font-bold text-foreground">{problem.name}</h2>
                <Chip color={getDifficultyColor(problem.difficulty)} variant="flat" size="md" className="font-medium">
                  {getDifficultyLabel(problem.difficulty)}
                </Chip>
              </div>
            </CardHeader>
            <CardBody className="pt-0">
              {problem.tags && problem.tags.length > 0 && (
                <div className="flex flex-wrap gap-2 mb-4">
                  {problem.tags.map((tag, index) => (
                    <Chip key={index} size="sm" variant="bordered" color="secondary" className="text-xs">
                      {tag.name}
                    </Chip>
                  ))}
                </div>
              )}

              <div className="space-y-4">
                <div>
                  <h3 className="text-lg font-semibold mb-2 text-foreground">Description</h3>
                  <p className="text-default-600 leading-relaxed">{problem.description}</p>
                </div>

                <Divider />

                <div className="space-y-4">
                  <div>
                    <h3 className="text-lg font-semibold mb-2 text-foreground">Input</h3>
                    <CodeBlock className="w-full overflow-x-auto">{formatValue(problem.input)}</CodeBlock>
                  </div>

                  <div>
                    <h3 className="text-lg font-semibold mb-2 text-foreground">Expected Output</h3>
                    <CodeBlock className="w-full overflow-x-auto">{formatValue(problem.expectedOutput)}</CodeBlock>
                  </div>

                  {problem.hints && problem.hints.length > 0 && (
                    <div>
                      <div className="flex justify-between items-center mb-2">
                        <h3 className="text-lg font-semibold text-foreground">Hints</h3>
                        <Button
                          size="sm"
                          variant="ghost"
                          color="secondary"
                          onPress={() => setShowHints(!showHints)}
                          startContent={
                            showHints ? <EyeSlashIcon className="h-4 w-4" /> : <EyeIcon className="h-4 w-4" />
                          }
                        >
                          {showHints ? 'Hide Hints' : 'Show Hints'}
                        </Button>
                      </div>
                      {showHints && (
                        <div className="space-y-2">
                          {problem.hints.map((hint, index) => (
                            <div key={index} className="p-3 bg-warning/10 border border-warning/20 rounded-lg">
                              <p className="text-warning-700 dark:text-warning-300">
                                <strong>Hint {index + 1}:</strong> {hint}
                              </p>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </CardBody>
          </Card>

          {submissionError && <ApiErrorDisplay error={submissionError} title="Submission Failed" showDetails={true} />}

          {result && (
            <Card className={`border-2 ${result.pass ? 'border-success' : 'border-danger'}`}>
              <CardHeader className="pb-3">
                <h4 className={`text-xl font-bold ${result.pass ? 'text-success' : 'text-danger'}`}>
                  {result.pass ? '✅ Success!' : '❌ Failed'}
                </h4>
              </CardHeader>
              <CardBody className="pt-0">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
                  <div className="text-center">
                    <div className="text-sm text-default-600">Test Cases</div>
                    <div className="text-lg font-semibold">{result.testCaseCount}</div>
                  </div>
                  <div className="text-center">
                    <div className="text-sm text-default-600">Passed</div>
                    <div className="text-lg font-semibold text-success">{result.passedTestCases}</div>
                  </div>
                  <div className="text-center">
                    <div className="text-sm text-default-600">Failed</div>
                    <div className="text-lg font-semibold text-danger">{result.failedTestCases}</div>
                  </div>
                  <div className="text-center">
                    <div className="text-sm text-default-600">Execution Time</div>
                    <div className="text-lg font-semibold">{result.executionTimeInMs}ms</div>
                  </div>
                </div>

                {result.errorMessage && (
                  <div className="mt-4">
                    <h5 className="text-danger font-semibold mb-2">Error Message:</h5>
                    <CodeBlock className="w-full text-danger">{result.errorMessage}</CodeBlock>
                  </div>
                )}
              </CardBody>
            </Card>
          )}
        </div>

        <div className="lg:col-span-2 space-y-4">
          <Card>
            <CardHeader className="pb-3">
              <div className="flex justify-between items-center w-full">
                <div className="flex items-center gap-3 min-w-0 flex-1">
                  <h3 className="text-xl font-semibold whitespace-nowrap">Code Editor</h3>
                  <Select
                    label="Language"
                    selectedKeys={selectedLanguage ? [selectedLanguage] : []}
                    onSelectionChange={keys => {
                      const newLanguage = Array.from(keys)[0] as Language;
                      setSelectedLanguage(newLanguage);
                      const selectedDriver = problem.drivers.find(driver => driver.language === newLanguage);
                      if (selectedDriver) {
                        // Try to restore saved code for the new language
                        const { code: savedCode } = loadCodeWithPriority(user?.id || null, problem.id, newLanguage);

                        // Use saved code if available, otherwise use the template
                        setCode(savedCode || selectedDriver.uiTemplate);
                      }
                    }}
                    className="w-32 flex-shrink-0"
                    size="sm"
                  >
                    {problem.drivers.map(driver => (
                      <SelectItem key={driver.language}>{driver.language}</SelectItem>
                    ))}
                  </Select>
                </div>
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
            </CardHeader>
            <CardBody className="pt-0">
              <div className="space-y-4">
                <CodeEditor
                  language={selectedLanguage || Language.JavaScript}
                  code={code}
                  onChange={value => setCode(value ?? '')}
                  uiTemplate={problem.drivers.find(d => d.language === selectedLanguage)?.uiTemplate ?? ''}
                  flamesEnabled={flamesEnabled}
                />

                <div className="flex justify-end">
                  <Button
                    color="primary"
                    size="lg"
                    isLoading={submitSolution.isPending}
                    isDisabled={!code.trim() || !selectedLanguage}
                    onPress={handleSubmit}
                    className="font-semibold"
                  >
                    {submitSolution.isPending ? 'Submitting...' : 'Submit Solution'}
                  </Button>
                </div>
              </div>
            </CardBody>
          </Card>
        </div>
      </div>

      {/* Login Modal */}
      <Modal isOpen={isOpen} onOpenChange={onOpenChange} placement="center">
        <ModalContent>
          {onClose => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                <h2 className="text-2xl font-bold text-primary">LOL!!!</h2>
              </ModalHeader>
              <ModalBody>
                <p className="text-default-600">
                  You thought I'd let you use my compute resources without signing in?
                  <br />
                  <br />
                  Either login, sign up, or gtfo 🖕
                  <br />
                  <br />
                  (Your code is saved locally)
                </p>
              </ModalBody>
              <ModalFooter>
                <Button color="default" variant="light" onPress={onClose}>
                  Cancel
                </Button>
                <Button
                  color="primary"
                  variant="ghost"
                  onPress={() => {
                    onClose();
                    navigate('/login');
                  }}
                >
                  Sign In
                </Button>
                <Button
                  color="primary"
                  onPress={() => {
                    onClose();
                    navigate('/register');
                  }}
                >
                  Sign Up
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>

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
    </div>
  );
};

import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import { Button, Spinner, useDisclosure } from '@heroui/react';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import { Language } from '../../types/models';
import type { SubmissionResult } from '../../types/models';
import { ApiErrorDisplay } from '../ApiErrorDisplay';
import { ProblemDescription } from './ProblemDescription';
import { ProblemCodeEditor } from './ProblemCodeEditor';
import { ProblemSubmissionResult } from './ProblemSubmissionResult';
import { ProblemModals } from './ProblemModals';
import { useDeleteProblem, useProblem, useSubmitSolution, useRunSolution, useCloneProblem } from '../../hooks/api';
import { useAuth } from '../../hooks/useAuth';
import { useAutoSave } from '../../hooks/useAutoSave';
import { loadCodeWithPriority, cleanupOldAutoSaveData } from '../../utils/localStorageUtils';

export const ProblemView = () => {
  const { id } = useParams<{ id: string }>();
  const { user, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const { data: problem, isLoading, error } = useProblem(Number(id!));
  const submitSolution = useSubmitSolution(problem?.id || 0);
  const runSolution = useRunSolution(problem?.id || 0);
  const deleteProblem = useDeleteProblem();
  const cloneProblem = useCloneProblem();
  const { isOpen, onOpen, onOpenChange } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onOpenChange: onDeleteOpenChange } = useDisclosure();
  const { isOpen: isResetOpen, onOpen: onResetOpen, onOpenChange: onResetOpenChange } = useDisclosure();

  const [selectedLanguage, setSelectedLanguage] = useState<Language | undefined>(undefined);
  const [code, setCode] = useState('');
  const [result, setResult] = useState<SubmissionResult | null>(null);
  const [submissionError, setSubmissionError] = useState<Error | null>(null);
  const [isRunResult, setIsRunResult] = useState(false); // Track if current result is from run or submit
  const [problemToDelete, setProblemToDelete] = useState<{ id: number; name: string } | null>(null);

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
        language: selectedLanguage
      });
      setResult(submissionResult);
      setIsRunResult(false); // Mark this as a submission result
    } catch (error) {
      console.error('Submission error:', error);
      setSubmissionError(error as Error);
      setResult(null); // Clear any previous results
      setIsRunResult(false);
    }
  };

  const handleRun = async () => {
    if (!problem || !selectedLanguage) return;

    // Check if user is authenticated
    if (!isAuthenticated || !user) {
      onOpen(); // Show login modal
      return;
    }

    try {
      setSubmissionError(null); // Clear any previous errors
      const runResult = await runSolution.mutateAsync({
        userCode: code,
        language: selectedLanguage
      });
      setResult(runResult);
      setIsRunResult(true); // Mark this as a run result
    } catch (error) {
      console.error('Run error:', error);
      setSubmissionError(error as Error);
      setResult(null); // Clear any previous results
      setIsRunResult(false);
    }
  };

  // Handle delete problem
  const handleDeleteClick = (problem: { id: number; name: string }) => {
    setProblemToDelete(problem);
    onDeleteOpen();
  };

  const handleConfirmDelete = async () => {
    if (!problemToDelete) return;

    try {
      await deleteProblem.mutateAsync(problemToDelete.id);
      setProblemToDelete(null);
      onDeleteOpenChange();
      navigate('/problems');
    } catch (error) {
      console.error('Failed to delete problem:', error);
      // Error handling could be enhanced with toast notifications
    }
  };

  const handleCancelDelete = () => {
    setProblemToDelete(null);
    onDeleteOpenChange();
  };

  // Handle clone problem
  const handleCloneProblem = async () => {
    if (!problem) return;

    try {
      const clonedProblem = await cloneProblem.mutateAsync(problem.id);
      navigate(`/problems/${clonedProblem.id}`);
    } catch (error) {
      console.error('Failed to clone problem:', error);
      // Error handling could be enhanced with toast notifications
    }
  };

  // Handle reset code to template
  const handleResetCode = () => {
    if (!problem || !selectedLanguage) return;

    const selectedDriver = problem.drivers.find(d => d.language === selectedLanguage);
    if (selectedDriver) {
      setCode(selectedDriver.uiTemplate);
      onResetOpenChange(); // Close the modal
    }
  };

  const handleLanguageChange = (language: Language) => {
    setSelectedLanguage(language);
  };

  const handleCodeChange = (newCode: string) => {
    setCode(newCode);
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
      <div className="max-w-md mx-auto">
        <div className="text-center py-8">
          <p className="text-warning text-lg">Problem not found</p>
        </div>
      </div>
    );
  }

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
          <ProblemDescription
            problem={problem}
            user={user}
            onEdit={() => navigate(`/problems/${problem.id}/edit`)}
            onClone={handleCloneProblem}
            onDelete={handleDeleteClick}
            isCloneLoading={cloneProblem.isPending}
          />

          {submissionError && (
            <ApiErrorDisplay
              error={submissionError}
              title={isRunResult ? 'Run Failed' : 'Submission Failed'}
              showDetails={true}
            />
          )}

          {result && <ProblemSubmissionResult result={result} isRunResult={isRunResult} />}
        </div>

        <div className="lg:col-span-2 space-y-4">
          <ProblemCodeEditor
            problem={problem}
            user={user}
            selectedLanguage={selectedLanguage}
            code={code}
            onLanguageChange={handleLanguageChange}
            onCodeChange={handleCodeChange}
            onSubmit={handleSubmit}
            onRun={handleRun}
            onReset={onResetOpen}
            isSubmitting={submitSolution.isPending}
            isRunning={runSolution.isPending}
          />
        </div>
      </div>

      <ProblemModals
        isLoginOpen={isOpen}
        onLoginOpenChange={onOpenChange}
        onNavigateToLogin={() => navigate('/login')}
        onNavigateToRegister={() => navigate('/register')}
        isDeleteOpen={isDeleteOpen}
        onDeleteOpenChange={onDeleteOpenChange}
        problemToDelete={problemToDelete}
        onConfirmDelete={handleConfirmDelete}
        onCancelDelete={handleCancelDelete}
        isDeleting={deleteProblem.isPending}
        isResetOpen={isResetOpen}
        onResetOpenChange={onResetOpenChange}
        problemName={problem.name}
        onConfirmReset={handleResetCode}
      />
    </div>
  );
};

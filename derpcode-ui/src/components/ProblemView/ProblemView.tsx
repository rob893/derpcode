import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';
import { Button, Spinner, useDisclosure } from '@heroui/react';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';
import { Language } from '../../types/models';
import type { ProblemSubmission } from '../../types/models';
import { ApiErrorDisplay } from '../ApiErrorDisplay';
import { ResizableSplitter } from '../ResizableSplitter';
import { ProblemDescription } from './ProblemDescription';
import { ProblemCodeEditor } from './ProblemCodeEditor';
import { ProblemSubmissionResult } from './ProblemSubmissionResult';
import { ProblemModals } from './ProblemModals';
import { useDeleteProblem, useProblem, useSubmitSolution, useRunSolution, useCloneProblem } from '../../hooks/api';
import { useAuth } from '../../hooks/useAuth';
import { useCurrentUser } from '../../hooks/useUser';
import { useAutoSave } from '../../hooks/useAutoSave';
import { loadCodeWithPriority, cleanupOldAutoSaveData } from '../../utils/localStorageUtils';

export const ProblemView = () => {
  const { id } = useParams<{ id: string }>();
  const { user, isAuthenticated } = useAuth();
  const { data: currentUserData } = useCurrentUser(); // Get full user data with emailConfirmed
  const navigate = useNavigate();
  const { data: problem, isLoading, error } = useProblem(Number(id!));
  const submitSolution = useSubmitSolution(user?.id || 0, problem?.id || 0);
  const runSolution = useRunSolution(problem?.id || 0);
  const deleteProblem = useDeleteProblem();
  const cloneProblem = useCloneProblem();
  const { isOpen, onOpen, onOpenChange } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onOpenChange: onDeleteOpenChange } = useDisclosure();
  const { isOpen: isResetOpen, onOpen: onResetOpen, onOpenChange: onResetOpenChange } = useDisclosure();
  const {
    isOpen: isEmailVerificationOpen,
    onOpen: onEmailVerificationOpen,
    onOpenChange: onEmailVerificationOpenChange
  } = useDisclosure();

  const [selectedLanguage, setSelectedLanguage] = useState<Language | undefined>(undefined);
  const [code, setCode] = useState('');
  const [result, setResult] = useState<ProblemSubmission | null>(null);
  const [submissionError, setSubmissionError] = useState<Error | null>(null);
  const [isRunResult, setIsRunResult] = useState(false); // Track if current result is from run or submit
  const [problemToDelete, setProblemToDelete] = useState<{ id: number; name: string } | null>(null);
  const [selectedSubmission, setSelectedSubmission] = useState<ProblemSubmission | null>(null);
  const [userWorkingCode, setUserWorkingCode] = useState(''); // Store user's working code when viewing submissions
  const [userWorkingLanguage, setUserWorkingLanguage] = useState<Language | undefined>(undefined); // Store user's working language when viewing submissions

  // Auto-save functionality (disabled when viewing submissions)
  useAutoSave(
    selectedSubmission ? '' : code, // Don't auto-save when viewing a submission
    selectedSubmission ? null : user?.id || null, // Disable auto-save when viewing a submission
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

  // Handle submission selection - load submission code into editor
  useEffect(() => {
    if (selectedSubmission) {
      setSelectedLanguage(selectedSubmission.language);
      setCode(selectedSubmission.code);
    }
  }, [selectedSubmission]);

  const handleSubmit = async () => {
    if (!problem || !selectedLanguage) return;

    // Check if user is authenticated
    if (!isAuthenticated || !user) {
      onOpen(); // Show login modal
      return;
    }

    // Check if user's email is verified
    if (currentUserData && !currentUserData.emailConfirmed) {
      onEmailVerificationOpen(); // Show email verification modal
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

  const handleSubmissionSelect = (submission: ProblemSubmission) => {
    // Save the current working code and language before switching to submission view
    setUserWorkingCode(code);
    setUserWorkingLanguage(selectedLanguage);
    setSelectedSubmission(submission);
  };

  const handleReturnToWorkingCode = () => {
    if (!problem) return;

    setSelectedSubmission(null);

    // Restore the user's working language first
    const languageToRestore = userWorkingLanguage || selectedLanguage;
    if (languageToRestore) {
      setSelectedLanguage(languageToRestore);
    }

    // Restore the user's working code if it exists, otherwise restore saved code or template
    if (userWorkingCode) {
      setCode(userWorkingCode);
    } else if (languageToRestore) {
      const { code: savedCode } = loadCodeWithPriority(user?.id || null, problem.id, languageToRestore);
      const selectedDriver = problem.drivers.find(d => d.language === languageToRestore);
      setCode(savedCode || selectedDriver?.uiTemplate || '');
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

      <div className="min-h-[calc(100vh-12rem)]">
        {/* Mobile layout: stack vertically */}
        <div className="flex flex-col space-y-6 lg:hidden">
          <div className="space-y-6">
            <ProblemDescription
              problem={problem}
              user={user}
              onEdit={() => navigate(`/problems/${problem.id}/edit`)}
              onClone={handleCloneProblem}
              onDelete={handleDeleteClick}
              isCloneLoading={cloneProblem.isPending}
              onSubmissionSelect={handleSubmissionSelect}
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

          <div>
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
              selectedSubmission={selectedSubmission}
              onReturnToWorkingCode={handleReturnToWorkingCode}
            />
          </div>
        </div>

        {/* Desktop layout: resizable horizontal split */}
        <div className="hidden lg:block">
          <ResizableSplitter
            leftPanel={
              <div className="space-y-6 pr-6">
                <ProblemDescription
                  problem={problem}
                  user={user}
                  onEdit={() => navigate(`/problems/${problem.id}/edit`)}
                  onClone={handleCloneProblem}
                  onDelete={handleDeleteClick}
                  isCloneLoading={cloneProblem.isPending}
                  onSubmissionSelect={handleSubmissionSelect}
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
            }
            rightPanel={
              <div className="pl-6">
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
                  selectedSubmission={selectedSubmission}
                  onReturnToWorkingCode={handleReturnToWorkingCode}
                />
              </div>
            }
            defaultLeftWidth={50}
            minLeftWidth={25}
            maxLeftWidth={75}
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
        isEmailVerificationOpen={isEmailVerificationOpen}
        onEmailVerificationOpenChange={onEmailVerificationOpenChange}
        onNavigateToAccount={() => navigate('/account')}
      />
    </div>
  );
};

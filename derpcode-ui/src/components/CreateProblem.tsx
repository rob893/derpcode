import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import {
  type CreateProblemDriverRequest,
  type CreateProblemRequest,
  type CreateProblemValidationResponse,
  Language,
  ProblemDifficulty
} from '../types/models';
import { CodeEditor } from './CodeEditor';
import { ApiErrorDisplay } from './ApiErrorDisplay';
import { useDriverTemplates, useCreateProblem, useValidateProblem } from '../hooks/api';
import { Button, Card, CardBody, CardHeader, Input, Textarea, Select, SelectItem, Chip, Spinner } from '@heroui/react';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';

export const CreateProblem = () => {
  const navigate = useNavigate();
  const { data: driverTemplates = [], isLoading, error } = useDriverTemplates();
  const createProblem = useCreateProblem();
  const validateProblem = useValidateProblem();
  const [selectedLanguage, setSelectedLanguage] = useState<Language>(Language.JavaScript);

  const [problem, setProblem] = useState<Partial<CreateProblemRequest>>({
    name: '',
    description: '',
    difficulty: ProblemDifficulty.Easy,
    tags: [],
    input: [],
    expectedOutput: [],
    hints: [],
    drivers: []
  });

  const [driverCode, setDriverCode] = useState('');
  const [uiTemplate, setUITemplate] = useState('');
  const [answer, setAnswer] = useState('');
  const [tagInput, setTagInput] = useState('');
  const [hintInput, setHintInput] = useState('');
  const [problemInput, setProblemInput] = useState('');
  const [problemExpectedOutput, setProblemExpectedOutput] = useState('');

  // Validation state
  const [isValidated, setIsValidated] = useState(false);
  const [validationErrors, setValidationErrors] = useState<string[]>([]);
  const [validationResult, setValidationResult] = useState<CreateProblemValidationResponse | null>(null);
  const [validationError, setValidationError] = useState<Error | null>(null);
  const [createError, setCreateError] = useState<Error | null>(null);

  // Set initial driver code from first available template
  useEffect(() => {
    if (driverTemplates.length > 0) {
      const firstLanguage = driverTemplates[0].language;
      setSelectedLanguage(firstLanguage);
      setDriverCode(driverTemplates[0].template);
      setUITemplate(driverTemplates[0].uiTemplate);
      setAnswer(driverTemplates[0].uiTemplate); // Initialize answer with the template
    }
  }, [driverTemplates]);

  // Reset validation when form changes
  useEffect(() => {
    setIsValidated(false);
    setValidationErrors([]);
    setValidationResult(null);
    setValidationError(null);
    setCreateError(null);
  }, [problem, driverCode, uiTemplate, answer, selectedLanguage]);

  const handleLanguageChange = (newLanguage: Language) => {
    setSelectedLanguage(newLanguage);
    const template = driverTemplates.find(x => x.language === newLanguage);
    setDriverCode(template?.template || '');
    setUITemplate(template?.uiTemplate || '');
    setAnswer(template?.uiTemplate || ''); // Update answer when language changes
  };

  const handleAddTag = () => {
    if (tagInput.trim() && !problem.tags?.map(x => x.name).includes(tagInput.trim())) {
      setProblem(prev => ({
        ...prev,
        tags: [...(prev.tags || []), { name: tagInput.trim() }]
      }));
      setTagInput('');
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setProblem(prev => ({
      ...prev,
      tags: prev.tags?.filter(tag => tag.name !== tagToRemove) || []
    }));
  };

  const handleAddHint = () => {
    if (hintInput.trim() && !problem.hints?.includes(hintInput.trim())) {
      setProblem(prev => ({
        ...prev,
        hints: [...(prev.hints || []), hintInput.trim()]
      }));
      setHintInput('');
    }
  };

  const handleRemoveHint = (hintToRemove: string) => {
    setProblem(prev => ({
      ...prev,
      hints: prev.hints?.filter(hint => hint !== hintToRemove) || []
    }));
  };

  const handleValidate = async () => {
    try {
      setValidationError(null); // Clear any previous errors
      // Create a driver for the current language
      const driver: CreateProblemDriverRequest = {
        language: selectedLanguage,
        image: `code-executor-${selectedLanguage.toLowerCase()}`,
        driverCode,
        uiTemplate,
        answer
      };

      const newProblem: CreateProblemRequest = {
        ...(problem as CreateProblemRequest),
        drivers: [driver]
      };

      const validationResponse = await validateProblem.mutateAsync(newProblem);
      setValidationResult(validationResponse);

      if (validationResponse.isValid) {
        setIsValidated(true);
        setValidationErrors([]);
      } else {
        setIsValidated(false);
        const errors = [
          validationResponse.errorMessage,
          ...validationResponse.driverValidations
            .filter(dv => !dv.isValid)
            .map(dv => `${dv.language}: ${dv.errorMessage}`)
        ].filter(Boolean) as string[];
        setValidationErrors(errors);
      }
    } catch (err) {
      console.error('Failed to validate problem:', err);
      setValidationError(err as Error);
      setIsValidated(false);
      setValidationErrors([]);
    }
  };

  const handleSubmit = async () => {
    try {
      setCreateError(null); // Clear any previous errors
      // Create a driver for the current language
      const driver: CreateProblemDriverRequest = {
        language: selectedLanguage,
        image: `code-executor-${selectedLanguage.toLowerCase()}`,
        driverCode,
        uiTemplate,
        answer
      };

      const newProblem: CreateProblemRequest = {
        ...(problem as CreateProblemRequest),
        drivers: [driver]
      };

      const createdProblem = await createProblem.mutateAsync(newProblem);
      navigate(`/problems/${createdProblem.id}`);
    } catch (err) {
      console.error('Failed to create problem:', err);
      setCreateError(err as Error);
    }
  };

  console.log(!problem.name, !problem.description, !driverCode, !answer, !isValidated);

  if (isLoading)
    return (
      <div className="flex justify-center items-center min-h-screen">
        <Spinner size="lg" color="primary" />
      </div>
    );

  if (error)
    return (
      <div className="flex justify-center items-center min-h-screen">
        <ApiErrorDisplay
          error={error}
          title="Failed to load driver templates"
          className="max-w-md"
          showDetails={true}
        />
      </div>
    );

  return (
    <div className="max-w-7xl mx-auto p-6 space-y-6">
      <div className="flex items-center gap-4">
        <Button
          isIconOnly
          variant="light"
          onPress={() => navigate('/')}
          startContent={<ArrowLeftIcon className="w-5 h-5" />}
        />
        <h2 className="text-3xl font-bold text-white">Create New Problem</h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <h3 className="text-xl font-semibold">Problem Details</h3>
            </CardHeader>
            <CardBody className="space-y-4">
              <Input
                label="Name"
                placeholder="Problem name"
                value={problem.name}
                onChange={e => setProblem(prev => ({ ...prev, name: e.target.value }))}
                variant="bordered"
                color="primary"
              />

              <Textarea
                label="Description"
                placeholder="Problem description"
                value={problem.description}
                onChange={e => setProblem(prev => ({ ...prev, description: e.target.value }))}
                variant="bordered"
                color="primary"
                minRows={4}
              />

              <Select
                label="Difficulty"
                selectedKeys={[problem.difficulty || ProblemDifficulty.Easy]}
                onSelectionChange={keys => {
                  const difficulty = Array.from(keys)[0] as ProblemDifficulty;
                  setProblem(prev => ({ ...prev, difficulty }));
                }}
                variant="bordered"
                color="primary"
              >
                <SelectItem key={ProblemDifficulty.VeryEasy}>Very Easy</SelectItem>
                <SelectItem key={ProblemDifficulty.Easy}>Easy</SelectItem>
                <SelectItem key={ProblemDifficulty.Medium}>Medium</SelectItem>
                <SelectItem key={ProblemDifficulty.Hard}>Hard</SelectItem>
                <SelectItem key={ProblemDifficulty.VeryHard}>Very Hard</SelectItem>
              </Select>

              <div className="space-y-2">
                <div className="flex gap-2">
                  <Input
                    placeholder="Add tag and press Enter"
                    value={tagInput}
                    onChange={e => setTagInput(e.target.value)}
                    onKeyDown={e => e.key === 'Enter' && handleAddTag()}
                    variant="bordered"
                    color="primary"
                    className="flex-1"
                  />
                  <Button color="primary" onPress={handleAddTag}>
                    Add Tag
                  </Button>
                </div>
                <div className="flex flex-wrap gap-2">
                  {problem.tags?.map(tag => (
                    <Chip key={tag.name} onClose={() => handleRemoveTag(tag.name)} variant="flat" color="primary">
                      {tag.name}
                    </Chip>
                  ))}
                </div>
              </div>

              <div className="space-y-2">
                <div className="flex gap-2">
                  <Input
                    placeholder="Add hint and press Enter"
                    value={hintInput}
                    onChange={e => setHintInput(e.target.value)}
                    onKeyDown={e => e.key === 'Enter' && handleAddHint()}
                    variant="bordered"
                    color="secondary"
                    className="flex-1"
                  />
                  <Button color="secondary" onPress={handleAddHint}>
                    Add Hint
                  </Button>
                </div>
                <div className="flex flex-wrap gap-2">
                  {problem.hints?.map((hint, index) => (
                    <Chip key={index} onClose={() => handleRemoveHint(hint)} variant="flat" color="secondary">
                      {hint}
                    </Chip>
                  ))}
                </div>
              </div>
            </CardBody>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader>
              <h3 className="text-xl font-semibold">Test Data</h3>
            </CardHeader>
            <CardBody className="space-y-4">
              <Textarea
                label="Input Array (JSON)"
                placeholder="[1, 2, 3]"
                value={problemInput}
                onChange={e => {
                  try {
                    setProblemInput(e.target.value);
                    const parsed = JSON.parse(e.target.value);
                    setProblem(prev => ({ ...prev, input: parsed }));
                  } catch {
                    // swallow invalid JSON error
                  }
                }}
                variant="bordered"
                color="primary"
                minRows={4}
              />

              <Textarea
                label="Expected Output Array (JSON)"
                placeholder="[3, 5, 7]"
                value={problemExpectedOutput}
                onChange={e => {
                  try {
                    setProblemExpectedOutput(e.target.value);
                    const parsed = JSON.parse(e.target.value);
                    setProblem(prev => ({ ...prev, expectedOutput: parsed }));
                  } catch {
                    // swallow invalid JSON error
                  }
                }}
                variant="bordered"
                color="primary"
                minRows={4}
              />
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <h3 className="text-xl font-semibold">Code Configuration</h3>
            </CardHeader>
            <CardBody>
              <Select
                label="Language"
                selectedKeys={[selectedLanguage]}
                onSelectionChange={keys => handleLanguageChange(Array.from(keys)[0] as Language)}
                variant="bordered"
                color="primary"
              >
                {driverTemplates.map(x => (
                  <SelectItem key={x.language}>{x.language}</SelectItem>
                ))}
              </Select>
            </CardBody>
          </Card>
        </div>
      </div>

      <div className="space-y-6">
        <Card>
          <CardHeader>
            <h3 className="text-xl font-semibold">UI Template</h3>
          </CardHeader>
          <CardBody className="p-2">
            <CodeEditor
              language={selectedLanguage}
              code={uiTemplate}
              onChange={value => setUITemplate(value ?? '')}
              uiTemplate=""
            />
          </CardBody>
        </Card>

        <Card>
          <CardHeader>
            <h3 className="text-xl font-semibold">Driver Code</h3>
          </CardHeader>
          <CardBody className="p-2">
            <CodeEditor
              language={selectedLanguage}
              code={driverCode}
              onChange={value => setDriverCode(value ?? '')}
              uiTemplate=""
            />
          </CardBody>
        </Card>

        <Card>
          <CardHeader>
            <h3 className="text-xl font-semibold">Default Solution</h3>
          </CardHeader>
          <CardBody className="p-2">
            <CodeEditor
              language={selectedLanguage}
              code={answer}
              onChange={value => setAnswer(value ?? '')}
              uiTemplate=""
            />
          </CardBody>
        </Card>
      </div>

      {/* API Error Display for Validation */}
      {validationError && (
        <ApiErrorDisplay error={validationError} title="Validation Request Failed" showDetails={true} />
      )}

      {/* API Error Display for Creation */}
      {createError && <ApiErrorDisplay error={createError} title="Problem Creation Failed" showDetails={true} />}

      {/* Validation Error Display */}
      {validationErrors.length > 0 && (
        <Card className="border-danger">
          <CardHeader className="pb-3">
            <h3 className="text-lg font-semibold text-danger">Validation Errors</h3>
          </CardHeader>
          <CardBody className="pt-0">
            <div className="space-y-2">
              {validationErrors.map((error, index) => (
                <div key={index} className="bg-danger/10 border border-danger/20 rounded-lg p-3">
                  <p className="text-danger text-sm">{error}</p>
                </div>
              ))}
            </div>
          </CardBody>
        </Card>
      )}

      {/* Validation Success Display */}
      {isValidated && validationResult?.isValid && (
        <Card className="border-success">
          <CardHeader className="pb-3">
            <h3 className="text-lg font-semibold text-success">✅ Validation Successful</h3>
          </CardHeader>
          <CardBody className="pt-0">
            <p className="text-success text-sm">
              All driver templates validated successfully. You can now create the problem.
            </p>
            {validationResult?.driverValidations?.map((dv: any, index: number) => (
              <div key={index} className="mt-2 p-3 bg-success/10 border border-success/20 rounded-lg">
                <p className="text-success text-sm">
                  <strong>{dv.language}:</strong> {dv.submissionResult.passedTestCases}/
                  {dv.submissionResult.testCaseCount} test cases passed
                </p>
              </div>
            ))}
          </CardBody>
        </Card>
      )}

      <div className="flex justify-end gap-3">
        <Button
          color="secondary"
          size="lg"
          onPress={handleValidate}
          isDisabled={!problem.name || !problem.description || !driverCode || !answer}
          isLoading={validateProblem.isPending}
          variant="bordered"
        >
          {validateProblem.isPending ? 'Validating...' : 'Validate'}
        </Button>

        <Button
          color="primary"
          size="lg"
          onPress={handleSubmit}
          isDisabled={!problem.name || !problem.description || !driverCode || !answer || !isValidated}
          isLoading={createProblem.isPending}
        >
          {createProblem.isPending ? 'Creating...' : 'Create Problem'}
        </Button>
      </div>
    </div>
  );
};

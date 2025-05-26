import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import {
  type CreateProblemDriverRequest,
  type CreateProblemRequest,
  Language,
  ProblemDifficulty
} from '../types/models';
import { CodeEditor } from './CodeEditor';
import { useDriverTemplates, useCreateProblem } from '../hooks/api';
import { Button, Card, CardBody, CardHeader, Input, Textarea, Select, SelectItem, Chip, Spinner } from '@heroui/react';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';

export const CreateProblem = () => {
  const navigate = useNavigate();
  const { data: driverTemplates = [], isLoading, error } = useDriverTemplates();
  const createProblem = useCreateProblem();
  const [selectedLanguage, setSelectedLanguage] = useState<Language>(Language.JavaScript);

  const [problem, setProblem] = useState<Partial<CreateProblemRequest>>({
    name: '',
    description: '',
    difficulty: ProblemDifficulty.Easy,
    tags: [],
    input: [],
    expectedOutput: [],
    drivers: []
  });

  const [driverCode, setDriverCode] = useState('');
  const [uiTemplate, setUITemplate] = useState('');
  const [tagInput, setTagInput] = useState('');
  const [problemInput, setProblemInput] = useState('');
  const [problemExpectedOutput, setProblemExpectedOutput] = useState('');

  // Set initial driver code from first available template
  useEffect(() => {
    if (driverTemplates.length > 0) {
      const firstLanguage = driverTemplates[0].language;
      setSelectedLanguage(firstLanguage);
      setDriverCode(driverTemplates[0].template);
      setUITemplate(driverTemplates[0].uiTemplate);
    }
  }, [driverTemplates]);

  const handleLanguageChange = (newLanguage: Language) => {
    setSelectedLanguage(newLanguage);
    setDriverCode(driverTemplates.find(x => x.language === newLanguage)?.template || '');
    setUITemplate(driverTemplates.find(x => x.language === newLanguage)?.uiTemplate || '');
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

  const handleSubmit = async () => {
    try {
      // Create a driver for the current language
      const driver: CreateProblemDriverRequest = {
        language: selectedLanguage,
        image: `code-executor-${selectedLanguage.toLowerCase()}`,
        driverCode,
        uiTemplate
      };

      const newProblem: CreateProblemRequest = {
        ...(problem as CreateProblemRequest),
        drivers: [driver]
      };

      const createdProblem = await createProblem.mutateAsync(newProblem);
      navigate(`/problems/${createdProblem.id}`);
    } catch (err) {
      console.error('Failed to create problem:', err);
    }
  };

  if (isLoading)
    return (
      <div className="flex justify-center items-center min-h-screen">
        <Spinner size="lg" color="primary" />
      </div>
    );

  if (error)
    return (
      <div className="flex justify-center items-center min-h-screen">
        <Card className="max-w-md">
          <CardBody className="text-center">
            <p className="text-danger">Error: {error.message}</p>
          </CardBody>
        </Card>
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
                  } catch (e) {
                    console.error('Invalid JSON:', e);
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
                  } catch (e) {
                    console.error('Invalid JSON:', e);
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
      </div>

      <div className="flex justify-end">
        <Button
          color="primary"
          size="lg"
          onPress={handleSubmit}
          isDisabled={!problem.name || !problem.description || !driverCode}
          isLoading={createProblem.isPending}
        >
          {createProblem.isPending ? 'Creating...' : 'Create Problem'}
        </Button>
      </div>
    </div>
  );
};

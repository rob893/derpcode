import { useState } from 'react';
import { Card, CardBody, CardHeader, Button, Chip, Divider, Code as CodeBlock, Tabs, Tab } from '@heroui/react';
import { EyeIcon, EyeSlashIcon, PencilIcon, TrashIcon, DocumentDuplicateIcon } from '@heroicons/react/24/outline';
import { ProblemDifficulty } from '../../types/models';
import type { Problem, ProblemSubmission } from '../../types/models';
import type { User } from '../../types/auth';
import { hasAdminRole, hasPremiumUserRole } from '../../utils/auth';
import { ProblemSubmissions } from './ProblemSubmissions';
import { MarkdownRenderer } from '../MarkdownRenderer';
import { ArticleComments } from '../ArticleComments';

interface ProblemDescriptionProps {
  problem: Problem;
  user: User | null;
  onEdit: () => void;
  onClone: () => void;
  onDelete: (problem: { id: number; name: string }) => void;
  isCloneLoading: boolean;
  onSubmissionSelect?: (submission: ProblemSubmission) => void;
}

export const ProblemDescription = ({
  problem,
  user,
  onEdit,
  onClone,
  onDelete,
  isCloneLoading,
  onSubmissionSelect
}: ProblemDescriptionProps) => {
  const [activeTab, setActiveTab] = useState('question');
  const [showHints, setShowHints] = useState(false);
  const [revealedHints, setRevealedHints] = useState<Set<number>>(new Set());

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

  const formatValue = (value: any): string => {
    return JSON.stringify(value, null, 2);
  };

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex justify-between items-start w-full">
          <h2 className="text-2xl font-bold text-foreground">{problem.name}</h2>
          <div className="flex items-center gap-2">
            {hasAdminRole(user) && (
              <div className="flex items-center gap-3">
                <Button
                  variant="ghost"
                  color="primary"
                  size="sm"
                  startContent={<PencilIcon className="h-4 w-4" />}
                  onPress={onEdit}
                >
                  Edit
                </Button>
                <Button
                  variant="ghost"
                  color="secondary"
                  size="sm"
                  startContent={<DocumentDuplicateIcon className="h-4 w-4" />}
                  onPress={onClone}
                  isLoading={isCloneLoading}
                >
                  Clone
                </Button>
                <Button
                  variant="ghost"
                  color="danger"
                  size="sm"
                  startContent={<TrashIcon className="h-4 w-4" />}
                  onPress={() => onDelete({ id: problem.id, name: problem.name })}
                >
                  Delete
                </Button>
              </div>
            )}
            <Chip color={getDifficultyColor(problem.difficulty)} variant="flat" size="md" className="font-medium">
              {getDifficultyLabel(problem.difficulty)}
            </Chip>
          </div>
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

        <Tabs
          selectedKey={activeTab}
          onSelectionChange={key => setActiveTab(key as string)}
          aria-label="Problem details tabs"
          color="primary"
          variant="underlined"
          classNames={{
            tabList: 'gap-6 w-full relative rounded-none p-0 border-b border-divider',
            cursor: 'w-full bg-primary',
            tab: 'max-w-fit px-0 h-12',
            tabContent: 'group-data-[selected=true]:text-primary'
          }}
        >
          <Tab key="question" title="Question">
            <div className="space-y-4 mt-4">
              <div>
                <h3 className="text-lg font-semibold mb-2 text-foreground">Description</h3>
                <MarkdownRenderer content={problem.description} />
              </div>

              <Divider />

              <div className="space-y-4">
                <div>
                  <div className="flex justify-between items-center mb-2">
                    <h3 className="text-lg font-semibold text-foreground">Input</h3>
                  </div>
                  {hasPremiumUserRole(user) ? (
                    <CodeBlock className="w-full overflow-x-auto">{formatValue(problem.input)}</CodeBlock>
                  ) : (
                    <div className="relative">
                      <CodeBlock className="w-full overflow-x-auto filter blur-xs">
                        {formatValue(problem.input)}
                      </CodeBlock>
                      <div className="absolute inset-0 flex items-center justify-center bg-black/30 rounded-lg">
                        <div className="text-center">
                          <p className="text-warning font-medium">Premium Feature</p>
                        </div>
                      </div>
                    </div>
                  )}
                </div>

                <div>
                  <div className="flex justify-between items-center mb-2">
                    <h3 className="text-lg font-semibold text-foreground">Expected Output</h3>
                  </div>
                  {hasPremiumUserRole(user) ? (
                    <CodeBlock className="w-full overflow-x-auto">{formatValue(problem.expectedOutput)}</CodeBlock>
                  ) : (
                    <div className="relative">
                      <CodeBlock className="w-full overflow-x-auto filter blur-xs">
                        {formatValue(problem.expectedOutput)}
                      </CodeBlock>
                      <div className="absolute inset-0 flex items-center justify-center bg-black/30 rounded-lg">
                        <div className="text-center">
                          <p className="text-warning font-medium">Premium Feature</p>
                        </div>
                      </div>
                    </div>
                  )}
                </div>

                {problem.hints && problem.hints.length > 0 && (
                  <div>
                    <div className="flex justify-between items-center mb-3">
                      <h3 className="text-lg font-semibold text-foreground">Hints</h3>
                      <Button
                        size="sm"
                        variant="ghost"
                        color="secondary"
                        onPress={() => {
                          setShowHints(!showHints);
                          if (!showHints) {
                            setRevealedHints(new Set()); // Reset revealed hints when showing
                          }
                        }}
                        startContent={
                          showHints ? <EyeSlashIcon className="h-4 w-4" /> : <EyeIcon className="h-4 w-4" />
                        }
                      >
                        {showHints ? 'Hide Hints' : 'Show Hints'}
                      </Button>
                    </div>
                    {showHints && (
                      <div className="space-y-3">
                        {problem.hints.map((hint, index) => (
                          <div key={index} className="space-y-2">
                            <div className="flex justify-between items-center">
                              <h4 className="text-medium font-semibold text-foreground">Hint {index + 1}</h4>
                              <Button
                                size="sm"
                                variant="bordered"
                                color="warning"
                                onPress={() => {
                                  const newRevealed = new Set(revealedHints);
                                  if (revealedHints.has(index)) {
                                    newRevealed.delete(index);
                                  } else {
                                    newRevealed.add(index);
                                  }
                                  setRevealedHints(newRevealed);
                                }}
                                startContent={
                                  revealedHints.has(index) ? (
                                    <EyeSlashIcon className="h-4 w-4" />
                                  ) : (
                                    <EyeIcon className="h-4 w-4" />
                                  )
                                }
                              >
                                {revealedHints.has(index) ? 'Hide' : 'Show'}
                              </Button>
                            </div>
                            {revealedHints.has(index) && (
                              <div className="p-3 bg-warning/10 border border-warning/20 rounded-lg">
                                <p className="text-warning-700 dark:text-warning-300">{hint}</p>
                              </div>
                            )}
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>
          </Tab>

          <Tab key="explanation" title="Explanation">
            <div className="space-y-4 mt-4">
              {!user ? (
                <div className="text-center py-8">
                  <p className="text-default-500 text-lg">Sign In Required</p>
                  <p className="text-default-400 text-sm mt-2">
                    You must be logged in to view the explanation for this problem.
                  </p>
                </div>
              ) : problem.explanationArticle ? (
                <div className="space-y-8">
                  <div>
                    <MarkdownRenderer content={problem.explanationArticle.content} />
                  </div>

                  <Divider />

                  {/* Article Comments Section */}
                  <div>
                    <ArticleComments articleId={problem.explanationArticle.id} user={user} />
                  </div>
                </div>
              ) : (
                <div className="text-center py-8">
                  <p className="text-default-500 text-lg">No Explanation Available</p>
                  <p className="text-default-400 text-sm mt-2">
                    The author hasn't provided an explanation for this problem yet.
                  </p>
                </div>
              )}
            </div>
          </Tab>

          <Tab key="solutions" title="Solutions">
            <div className="space-y-4 mt-4">
              {!user ? (
                <div className="text-center py-8">
                  <p className="text-default-500 text-lg">Sign In Required</p>
                  <p className="text-default-400 text-sm mt-2">
                    You must be logged in to view solutions for this problem.
                  </p>
                </div>
              ) : (
                <div className="text-center py-8">
                  <p className="text-default-500 text-lg">Coming Soon</p>
                  <p className="text-default-400 text-sm mt-2">
                    Sample solutions in different programming languages will be available here.
                  </p>
                </div>
              )}
            </div>
          </Tab>

          <Tab key="submissions" title="Submissions">
            <div className="space-y-4 mt-4">
              {onSubmissionSelect ? (
                <ProblemSubmissions problemId={problem.id} onSubmissionSelect={onSubmissionSelect} />
              ) : (
                <div className="text-center py-8">
                  <p className="text-default-500 text-lg">Coming Soon</p>
                  <p className="text-default-400 text-sm mt-2">
                    Your submission history and performance analytics will be available here.
                  </p>
                </div>
              )}
            </div>
          </Tab>
        </Tabs>
      </CardBody>
    </Card>
  );
};

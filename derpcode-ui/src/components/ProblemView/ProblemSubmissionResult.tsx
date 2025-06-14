import { Card, CardBody, CardHeader, Code as CodeBlock } from '@heroui/react';
import type { ProblemSubmission } from '../../types/models';

interface ProblemSubmissionResultProps {
  result: ProblemSubmission;
  isRunResult: boolean;
}

export const ProblemSubmissionResult = ({ result, isRunResult }: ProblemSubmissionResultProps) => {
  return (
    <Card
      className={`border-2 ${result.pass ? (isRunResult ? 'border-secondary' : 'border-success') : isRunResult ? 'border-warning' : 'border-danger'}`}
    >
      <CardHeader className="pb-3">
        <h4
          className={`text-xl font-bold ${result.pass ? (isRunResult ? 'text-secondary' : 'text-success') : isRunResult ? 'text-warning' : 'text-danger'}`}
        >
          {isRunResult
            ? result.pass
              ? '🔄 Run Complete!'
              : '⚠️ Run Failed'
            : result.pass
              ? '✅ Success!'
              : '❌ Failed'}
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
            <h5 className={`font-semibold mb-2 ${isRunResult ? 'text-warning' : 'text-danger'}`}>Error Message:</h5>
            <CodeBlock className={`w-full ${isRunResult ? 'text-warning' : 'text-danger'}`}>
              {result.errorMessage}
            </CodeBlock>
          </div>
        )}
      </CardBody>
    </Card>
  );
};

import { Card, CardBody, CardHeader, Divider } from '@heroui/react';
import type { ProblemSubmission } from '../../types/models';
import type { User } from '../../types/auth';
import { hasPremiumUserRole } from '../../utils/auth';
import { TestCaseDetails } from './TestCaseDetails';

interface ProblemSubmissionResultProps {
  result: ProblemSubmission;
  isRunResult: boolean;
  user: User | null;
}

export const ProblemSubmissionResult = ({ result, isRunResult, user }: ProblemSubmissionResultProps) => {
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
            <div
              className={`w-full p-3 rounded-lg bg-default-100 border border-default-200 ${isRunResult ? 'text-warning' : 'text-danger'} font-mono text-sm`}
            >
              <p>{result.errorMessage}</p>
            </div>
          </div>
        )}

        {/* Test Case Details - Premium Feature */}
        {result.testCaseResults && (
          <div className="mt-4">
            {hasPremiumUserRole(user) ? (
              <>
                <Divider className="mb-4" />
                <TestCaseDetails testCases={result.testCaseResults} />
              </>
            ) : (
              <>
                <Divider className="mb-4" />
                <Card className="border border-warning/20 bg-warning/5">
                  <CardBody className="text-center py-6">
                    <h5 className="text-lg font-semibold text-warning mb-2">🔒 Premium Feature</h5>
                    <p className="text-default-600 mb-2">
                      View detailed test case results including inputs, expected outputs, and your outputs.
                    </p>
                    <p className="text-sm text-default-500">
                      Upgrade to Premium to unlock this feature and enhance your debugging experience.
                    </p>
                  </CardBody>
                </Card>
              </>
            )}
          </div>
        )}
      </CardBody>
    </Card>
  );
};

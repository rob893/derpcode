import { useState } from 'react';
import { Card, CardBody, CardHeader, Button, Chip, Code as CodeBlock, Divider } from '@heroui/react';
import { ChevronDownIcon, ChevronUpIcon } from '@heroicons/react/24/outline';
import type { TestCaseResult } from '../../types/models';

interface TestCaseDetailsProps {
  testCases: TestCaseResult[];
}

export function TestCaseDetails({ testCases }: TestCaseDetailsProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [selectedTestCase, setSelectedTestCase] = useState<number | null>(null);

  if (!testCases || testCases.length === 0) {
    return null;
  }

  const failedTestCases = testCases.filter(tc => !tc.pass);
  const passedTestCases = testCases.filter(tc => tc.pass);

  const formatValue = (value: any): string => {
    if (value === null) return 'null';
    if (value === undefined) return 'undefined';
    if (typeof value === 'string') return `"${value}"`;
    if (typeof value === 'object') return JSON.stringify(value, null, 2);
    return String(value);
  };

  const renderTestCase = (testCase: TestCaseResult, index: number) => (
    <Card key={testCase.id} className={`border ${testCase.pass ? 'border-success-200' : 'border-danger-200'} mb-3`}>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between w-full">
          <div className="flex items-center gap-2">
            <h5 className="font-semibold">Test Case {index + 1}</h5>
            <Chip color={testCase.pass ? 'success' : 'danger'} variant="flat" size="sm">
              {testCase.pass ? 'Passed' : 'Failed'}
            </Chip>
          </div>
          <Button
            size="sm"
            variant="light"
            onPress={() => setSelectedTestCase(selectedTestCase === index ? null : index)}
            endContent={
              selectedTestCase === index ? (
                <ChevronUpIcon className="w-4 h-4" />
              ) : (
                <ChevronDownIcon className="w-4 h-4" />
              )
            }
          >
            {selectedTestCase === index ? 'Hide Details' : 'Show Details'}
          </Button>
        </div>
      </CardHeader>
      {selectedTestCase === index && (
        <CardBody className="pt-0">
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-default-600 mb-1 block">Input:</label>
              <CodeBlock className="w-full text-sm">{formatValue(testCase.input)}</CodeBlock>
            </div>

            <div>
              <label className="text-sm font-medium text-default-600 mb-1 block">Expected Output:</label>
              <CodeBlock className="w-full text-sm">{formatValue(testCase.expectedOutput)}</CodeBlock>
            </div>

            <div>
              <label className="text-sm font-medium text-default-600 mb-1 block">Your Output:</label>
              <CodeBlock className={`w-full text-sm ${testCase.pass ? '' : 'text-danger'}`}>
                {formatValue(testCase.actualOutput)}
              </CodeBlock>
            </div>

            <div>
              <label className="text-sm font-medium text-default-600 mb-1 block">Stdout:</label>
              <CodeBlock className="w-full text-sm">{formatValue(testCase.stdOut)}</CodeBlock>
            </div>

            {testCase.errorMessage && (
              <div>
                <label className="text-sm font-medium text-danger mb-1 block">Error:</label>
                <CodeBlock className="w-full text-danger text-sm">{testCase.errorMessage}</CodeBlock>
              </div>
            )}
          </div>
        </CardBody>
      )}
    </Card>
  );

  return (
    <Card className="mt-4">
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between w-full">
          <h4 className="text-lg font-semibold">Test Case Details</h4>
          <Button
            variant="light"
            onPress={() => setIsExpanded(!isExpanded)}
            endContent={isExpanded ? <ChevronUpIcon className="w-4 h-4" /> : <ChevronDownIcon className="w-4 h-4" />}
          >
            {isExpanded ? 'Hide All' : 'Show All'}
          </Button>
        </div>
      </CardHeader>
      {isExpanded && (
        <CardBody className="pt-0">
          {/* Summary */}
          <div className="mb-4 grid grid-cols-2 gap-4">
            <div className="text-center">
              <div className="text-sm text-default-600">Passed</div>
              <div className="text-lg font-semibold text-success">{passedTestCases.length}</div>
            </div>
            <div className="text-center">
              <div className="text-sm text-default-600">Failed</div>
              <div className="text-lg font-semibold text-danger">{failedTestCases.length}</div>
            </div>
          </div>

          <Divider className="mb-4" />

          {/* Failed test cases first */}
          {failedTestCases.length > 0 && (
            <div className="mb-6">
              <h5 className="text-md font-semibold text-danger mb-3">Failed Test Cases</h5>
              {failedTestCases.map(testCase => renderTestCase(testCase, testCases.indexOf(testCase)))}
            </div>
          )}

          {/* Passed test cases */}
          {passedTestCases.length > 0 && (
            <div>
              <h5 className="text-md font-semibold text-success mb-3">Passed Test Cases</h5>
              {passedTestCases.map(testCase => renderTestCase(testCase, testCases.indexOf(testCase)))}
            </div>
          )}
        </CardBody>
      )}
    </Card>
  );
}

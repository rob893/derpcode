import { useMemo, useState } from 'react';
import {
  Table,
  TableHeader,
  TableColumn,
  TableBody,
  TableRow,
  TableCell,
  Chip,
  Spinner,
  getKeyValue
} from '@heroui/react';
import { ClockIcon, CheckCircleIcon, XCircleIcon } from '@heroicons/react/24/outline';
import { useUserSubmissionsForProblem } from '../../hooks/api';
import { useAuth } from '../../hooks/useAuth';
import type { ProblemSubmission, Language } from '../../types/models';

interface ProblemSubmissionsProps {
  problemId: number;
  onSubmissionSelect: (submission: ProblemSubmission) => void;
}

type SortKey = 'language' | 'submittedAt' | 'executionTime';
type SortDirection = 'ascending' | 'descending';

export const ProblemSubmissions = ({ problemId, onSubmissionSelect }: ProblemSubmissionsProps) => {
  const { user } = useAuth();
  const [sortDescriptor, setSortDescriptor] = useState<{
    column: SortKey;
    direction: SortDirection;
  }>({
    column: 'submittedAt',
    direction: 'descending' // Default to most recent first
  });

  const {
    data: submissionsResponse,
    isLoading,
    error
  } = useUserSubmissionsForProblem(user?.id || 0, problemId, { first: 20, includeNodes: true });

  const submissions = useMemo(() => {
    const rawSubmissions = submissionsResponse?.nodes || [];

    // Sort submissions based on current sort descriptor
    const sorted = [...rawSubmissions].sort((a, b) => {
      let result = 0;

      switch (sortDescriptor.column) {
        case 'language':
          result = a.language.localeCompare(b.language);
          break;
        case 'submittedAt':
          result = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          break;
        case 'executionTime':
          result = a.executionTimeInMs - b.executionTimeInMs;
          break;
        default:
          // Default sort by most recent submission
          result = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      }

      return sortDescriptor.direction === 'descending' ? -result : result;
    });

    return sorted;
  }, [submissionsResponse, sortDescriptor]);

  const getLanguageLabel = (language: Language): string => {
    switch (language) {
      case 'CSharp':
        return 'C#';
      case 'JavaScript':
        return 'JavaScript';
      case 'TypeScript':
        return 'TypeScript';
      default:
        return language;
    }
  };

  const getStatusColor = (pass: boolean) => {
    return pass ? 'success' : 'danger';
  };

  const getStatusIcon = (pass: boolean) => {
    return pass ? <CheckCircleIcon className="h-4 w-4" /> : <XCircleIcon className="h-4 w-4" />;
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  };

  const formatExecutionTime = (timeInMs: number) => {
    if (timeInMs < 1000) {
      return `${timeInMs}ms`;
    }
    return `${(timeInMs / 1000).toFixed(2)}s`;
  };

  const columns = [
    { key: 'status', label: 'Status', sortable: false },
    { key: 'language', label: 'Language', sortable: true },
    { key: 'testCases', label: 'Test Cases', sortable: false },
    { key: 'executionTime', label: 'Runtime', sortable: true },
    { key: 'submittedAt', label: 'Submitted', sortable: true }
  ];

  const tableRows = useMemo(() => {
    return submissions.map(submission => ({
      key: submission.id,
      submission,
      status: (
        <Chip
          color={getStatusColor(submission.pass)}
          variant="flat"
          startContent={getStatusIcon(submission.pass)}
          size="sm"
        >
          {submission.pass ? 'Accepted' : 'Failed'}
        </Chip>
      ),
      language: (
        <Chip variant="bordered" size="sm" color="secondary">
          {getLanguageLabel(submission.language)}
        </Chip>
      ),
      testCases: `${submission.passedTestCases}/${submission.testCaseCount}`,
      executionTime: (
        <div className="flex items-center gap-1">
          <ClockIcon className="h-4 w-4 text-default-400" />
          <span>{formatExecutionTime(submission.executionTimeInMs)}</span>
        </div>
      ),
      submittedAt: formatDate(submission.createdAt)
    }));
  }, [submissions]);

  if (!user) {
    return (
      <div className="text-center py-8">
        <p className="text-default-500 text-lg">Please log in to view submissions</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex justify-center py-8">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-8">
        <p className="text-danger text-lg">Failed to load submissions</p>
        <p className="text-default-400 text-sm mt-2">Please try again later</p>
      </div>
    );
  }

  if (submissions.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-default-500 text-lg">No submissions yet</p>
        <p className="text-default-400 text-sm mt-2">Submit your solution to see your submission history here</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold">Your Submissions</h3>
        <p className="text-sm text-default-500">{submissions.length} submission(s)</p>
      </div>

      <Table
        aria-label="Submissions table"
        selectionMode="single"
        sortDescriptor={{
          column: sortDescriptor.column,
          direction: sortDescriptor.direction
        }}
        onSortChange={descriptor => {
          if (descriptor.column) {
            setSortDescriptor({
              column: descriptor.column as SortKey,
              direction: descriptor.direction as SortDirection
            });
          }
        }}
        onRowAction={key => {
          const submission = submissions.find(s => s.id === Number(key));
          if (submission) {
            onSubmissionSelect(submission);
          }
        }}
        classNames={{
          wrapper: 'rounded-lg',
          table: 'min-h-[200px]',
          thead: '[&>tr]:first:rounded-lg',
          tbody: '[&>tr:hover]:bg-default-100 cursor-pointer'
        }}
      >
        <TableHeader columns={columns}>
          {column => (
            <TableColumn key={column.key} className="bg-default-50" allowsSorting={column.sortable}>
              {column.label}
            </TableColumn>
          )}
        </TableHeader>
        <TableBody items={tableRows}>
          {item => (
            <TableRow key={item.key}>{columnKey => <TableCell>{getKeyValue(item, columnKey)}</TableCell>}</TableRow>
          )}
        </TableBody>
      </Table>
    </div>
  );
};

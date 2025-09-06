import { useNavigate } from 'react-router';
import { useState, useMemo, useCallback } from 'react';
import { Card, CardBody, Chip, Button, Spinner, Divider, Select, SelectItem, Input } from '@heroui/react';
import { FunnelIcon, ArrowPathIcon, MagnifyingGlassIcon } from '@heroicons/react/24/outline';
import { ProblemDifficulty, ProblemOrderBy, OrderByDirection } from '../types/models';
import { ApiErrorDisplay } from './ApiErrorDisplay';
import { useProblemsLimitedPaginated } from '../hooks/api';
import { useAuth } from '../hooks/useAuth';
import { hasAdminRole } from '../utils/auth';
import { useDebounce } from '../hooks/useDebounce';

export const ProblemList = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const isAdmin = hasAdminRole(user);

  const pageSize = 5;

  // State for filters and pagination
  const [selectedDifficulties, setSelectedDifficulties] = useState<Set<string>>(new Set());
  const [selectedTags, setSelectedTags] = useState<Set<string>>(new Set());
  const [showFilters, setShowFilters] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [sortBy, setSortBy] = useState<ProblemOrderBy>(ProblemOrderBy.Difficulty);
  const [sortDirection, setSortDirection] = useState<OrderByDirection>(OrderByDirection.Ascending);
  const [cursor, setCursor] = useState<string | undefined>(undefined);
  const [previousCursors, setPreviousCursors] = useState<string[]>([]);

  // Debounce search query to avoid too many API calls
  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  // Build query parameters
  const queryParams = useMemo(() => {
    // Reset cursor when filters change
    if (
      debouncedSearchQuery !== searchQuery ||
      Array.from(selectedDifficulties).join(',') !== Array.from(new Set()).join(',') ||
      Array.from(selectedTags).join(',') !== Array.from(new Set()).join(',')
    ) {
      // This indicates filters are changing, we should reset cursor
    }

    return {
      first: pageSize,
      after: cursor,
      includeTotal: true,
      includeUnpublished: isAdmin,
      difficulties: Array.from(selectedDifficulties) as ProblemDifficulty[],
      tags: Array.from(selectedTags),
      orderBy: sortBy,
      orderByDirection: sortDirection
      // TODO: Add search functionality to backend API
      // searchQuery: debouncedSearchQuery.trim() || undefined,
    };
  }, [
    pageSize,
    cursor,
    isAdmin,
    selectedDifficulties,
    selectedTags,
    sortBy,
    sortDirection,
    debouncedSearchQuery,
    searchQuery
  ]);

  const { data, isLoading, error } = useProblemsLimitedPaginated(queryParams);

  // Memoize problems to avoid dependency issues in other useMemo hooks
  const problems = useMemo(() => data?.problems || [], [data?.problems]);
  const pageInfo = data?.pageInfo;
  const totalCount = data?.totalCount || 0;

  // Apply client-side search filtering until backend supports search
  const filteredProblems = useMemo(() => {
    if (!debouncedSearchQuery.trim()) {
      return problems;
    }

    const searchTerm = debouncedSearchQuery.toLowerCase();
    return problems.filter(
      problem =>
        problem.name.toLowerCase().includes(searchTerm) ||
        problem.tags?.some(tag => tag.name.toLowerCase().includes(searchTerm))
    );
  }, [problems, debouncedSearchQuery]);

  // Pagination handlers
  const goToNextPage = useCallback(() => {
    if (pageInfo?.hasNextPage && pageInfo.endCursor) {
      setPreviousCursors(prev => [...prev, cursor || '']);
      setCursor(pageInfo.endCursor);
    }
  }, [pageInfo, cursor]);

  const goToPreviousPage = useCallback(() => {
    if (previousCursors.length > 0) {
      const newCursors = [...previousCursors];
      const previousCursor = newCursors.pop();
      setPreviousCursors(newCursors);
      setCursor(previousCursor === '' ? undefined : previousCursor);
    }
  }, [previousCursors]);

  // Clear all filters
  const clearFilters = useCallback(() => {
    setSearchQuery('');
    setSelectedDifficulties(new Set());
    setSelectedTags(new Set());
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  // Handle sort change
  const handleSortChange = useCallback((newSort: string) => {
    const [orderBy, direction] = newSort.split('-');
    setSortBy(orderBy as ProblemOrderBy);
    setSortDirection(direction === 'desc' ? OrderByDirection.Descending : OrderByDirection.Ascending);
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  // Handle filter changes
  const handleDifficultyChange = useCallback((keys: any) => {
    setSelectedDifficulties(new Set(Array.from(keys).map(String)));
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  const handleTagsChange = useCallback((keys: any) => {
    setSelectedTags(new Set(Array.from(keys).map(String)));
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  const handleSearchChange = useCallback((value: string) => {
    setSearchQuery(value);
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  // Get current sort string for select component
  const currentSortString = useMemo(() => {
    const direction = sortDirection === OrderByDirection.Descending ? 'desc' : 'asc';
    return `${sortBy}-${direction}`;
  }, [sortBy, sortDirection]);

  // Get all unique tags (we'll need to get this from a separate API call or store)
  // For now, let's create a placeholder that gets updated when we have more data
  const allTags = useMemo(() => {
    const tagSet = new Set<string>();
    problems.forEach(problem => {
      problem.tags?.forEach(tag => tagSet.add(tag.name));
    });
    return Array.from(tagSet).sort();
  }, [problems]);

  // Select random problem
  const selectRandomProblem = useCallback(() => {
    if (filteredProblems.length > 0) {
      const randomIndex = Math.floor(Math.random() * filteredProblems.length);
      const randomProblem = filteredProblems[randomIndex];
      navigate(`/problems/${randomProblem.id}`);
    }
  }, [filteredProblems, navigate]);

  // Helper functions for difficulty display
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

  if (isLoading) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <Spinner size="lg" color="primary" label="Loading problems..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-7xl mx-auto p-6">
        <ApiErrorDisplay
          error={error}
          title="Failed to load problems"
          className="max-w-md mx-auto"
          showDetails={true}
        />
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-3xl font-bold text-foreground">Problems</h2>
      </div>

      {/* Action buttons row with search bar in center */}
      <div className="flex justify-between items-center gap-4">
        <div className="flex items-center gap-4">
          <Button
            color="secondary"
            variant="ghost"
            size="md"
            onPress={selectRandomProblem}
            isDisabled={filteredProblems.length === 0}
            startContent={<ArrowPathIcon className="w-4 h-4" />}
            className="font-semibold shrink-0"
          >
            Random Problem
          </Button>

          {/* Search Bar - Left side next to Random Problem */}
          <div className="w-80">
            <Input
              placeholder="Search problems by name or tags..."
              value={searchQuery}
              onChange={e => handleSearchChange(e.target.value)}
              variant="bordered"
              size="md"
              startContent={<MagnifyingGlassIcon className="w-4 h-4 text-default-400" />}
              className="w-full"
              aria-label="Search problems"
              isClearable
              onClear={() => handleSearchChange('')}
            />
          </div>
        </div>

        <div className="flex items-center gap-3 shrink-0">
          {/* Sort selector */}
          <div className="flex items-center gap-2">
            <span className="text-md text-default-600">Sort by:</span>
            <Select
              selectedKeys={[currentSortString]}
              onSelectionChange={keys => handleSortChange(Array.from(keys)[0] as string)}
              className="w-40"
              variant="bordered"
              size="md"
              aria-label="Sort problems by"
              name="problem-sort"
            >
              <SelectItem key="Name-asc">Name (A → Z)</SelectItem>
              <SelectItem key="Name-desc">Name (Z → A)</SelectItem>
              <SelectItem key="Difficulty-asc">Difficulty (Easy → Hard)</SelectItem>
              <SelectItem key="Difficulty-desc">Difficulty (Hard → Easy)</SelectItem>
            </Select>
          </div>

          <Button
            color="default"
            variant="bordered"
            size="md"
            onPress={() => setShowFilters(!showFilters)}
            startContent={<FunnelIcon className="w-4 h-4" />}
            className="font-semibold"
          >
            {showFilters ? 'Hide Filters' : 'Show Filters'}
          </Button>
        </div>
      </div>

      <Divider />

      {/* Filters Section */}
      {showFilters && (
        <div className="space-y-4">
          <div className="flex flex-wrap gap-4 items-center">
            <div className="flex-1 min-w-0">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Difficulty Filter */}
                <div className="space-y-2">
                  <label className="text-md font-medium text-foreground">
                    Difficulty
                    <Select
                      placeholder="Select difficulties"
                      selectionMode="multiple"
                      selectedKeys={selectedDifficulties}
                      onSelectionChange={handleDifficultyChange}
                      className="w-full"
                      variant="bordered"
                      size="md"
                      aria-label="Filter by difficulty"
                    >
                      <SelectItem key={ProblemDifficulty.VeryEasy}>Very Easy</SelectItem>
                      <SelectItem key={ProblemDifficulty.Easy}>Easy</SelectItem>
                      <SelectItem key={ProblemDifficulty.Medium}>Medium</SelectItem>
                      <SelectItem key={ProblemDifficulty.Hard}>Hard</SelectItem>
                      <SelectItem key={ProblemDifficulty.VeryHard}>Very Hard</SelectItem>
                    </Select>
                  </label>
                </div>

                {/* Tags Filter */}
                <div className="space-y-2">
                  <label className="text-md font-medium text-foreground">
                    Tags
                    <Select
                      placeholder="Select tags"
                      selectionMode="multiple"
                      selectedKeys={selectedTags}
                      onSelectionChange={handleTagsChange}
                      className="w-full"
                      variant="bordered"
                      size="md"
                      aria-label="Filter by tags"
                    >
                      {allTags.map(tag => (
                        <SelectItem key={tag}>{tag}</SelectItem>
                      ))}
                    </Select>
                  </label>
                </div>
              </div>
            </div>

            {/* Clear Filters Button */}
            <div className="flex items-end">
              <Button
                variant="ghost"
                color="default"
                size="md"
                onPress={clearFilters}
                isDisabled={searchQuery.trim() === '' && selectedDifficulties.size === 0 && selectedTags.size === 0}
              >
                Clear Filters
              </Button>
            </div>
          </div>

          {/* Active Filters Display */}
          {(searchQuery.trim() !== '' || selectedDifficulties.size > 0 || selectedTags.size > 0) && (
            <div className="space-y-2">
              <div className="text-sm text-default-600">
                Active filters (showing {filteredProblems.length} results):
              </div>
              <div className="flex flex-wrap gap-2">
                {searchQuery.trim() !== '' && (
                  <Chip color="default" variant="flat" size="sm" onClose={() => handleSearchChange('')}>
                    Search: "{searchQuery}"
                  </Chip>
                )}
                {Array.from(selectedDifficulties).map(difficulty => (
                  <Chip
                    key={difficulty}
                    color="primary"
                    variant="flat"
                    size="sm"
                    onClose={() => {
                      const newSelection = new Set(selectedDifficulties);
                      newSelection.delete(difficulty);
                      setSelectedDifficulties(newSelection);
                    }}
                  >
                    Difficulty: {getDifficultyLabel(difficulty as ProblemDifficulty)}
                  </Chip>
                ))}
                {Array.from(selectedTags).map(tag => (
                  <Chip
                    key={tag}
                    color="secondary"
                    variant="flat"
                    size="sm"
                    onClose={() => {
                      const newSelection = new Set(selectedTags);
                      newSelection.delete(tag);
                      setSelectedTags(newSelection);
                    }}
                  >
                    Tag: {tag}
                  </Chip>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {!showFilters && <Divider />}

      {/* Pagination and Results Info */}
      <div className="flex justify-between items-center">
        <div className="text-sm text-default-600">
          Showing {filteredProblems.length} problems {totalCount ? `of ${totalCount} total` : ''}
        </div>
        <div className="flex items-center gap-2">
          <Button size="sm" variant="bordered" onPress={goToPreviousPage} isDisabled={previousCursors.length === 0}>
            Previous
          </Button>
          <Button size="sm" variant="bordered" onPress={goToNextPage} isDisabled={!pageInfo?.hasNextPage}>
            Next
          </Button>
        </div>
      </div>

      {/* Problems Grid */}
      <div className="grid gap-4">
        {filteredProblems.length === 0 ? (
          <Card className="p-8">
            <CardBody className="text-center">
              <div className="text-lg text-default-600 mb-2">No problems found</div>
              <div className="text-sm text-default-500">
                {searchQuery.trim() !== '' || selectedDifficulties.size > 0 || selectedTags.size > 0
                  ? 'Try adjusting your search or filters'
                  : 'No problems available yet'}
              </div>
            </CardBody>
          </Card>
        ) : (
          filteredProblems.map(problem => (
            <Card
              key={problem.id}
              isPressable
              isHoverable
              onPress={() => navigate(`/problems/${problem.id}`)}
              className="transition-all duration-200 hover:scale-[1.02]"
            >
              <CardBody className="p-6">
                <div className="flex justify-between items-start mb-3">
                  <h3 className="text-xl font-semibold text-foreground hover:text-primary transition-colors">
                    {problem.name}
                  </h3>
                  <div className="flex items-center gap-2">
                    <Chip
                      color={getDifficultyColor(problem.difficulty)}
                      variant="flat"
                      size="sm"
                      className="font-medium"
                    >
                      {getDifficultyLabel(problem.difficulty)}
                    </Chip>
                    {!problem.isPublished && (
                      <Chip color="warning" variant="flat" size="sm" className="font-medium">
                        Unpublished
                      </Chip>
                    )}
                  </div>
                </div>

                {problem.tags && problem.tags.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {problem.tags.map((tag: any, index: number) => (
                      <Chip key={index} size="sm" variant="bordered" color="secondary" className="text-xs">
                        {tag.name}
                      </Chip>
                    ))}
                  </div>
                )}
              </CardBody>
            </Card>
          ))
        )}
      </div>

      {/* Bottom pagination */}
      {(pageInfo?.hasNextPage || previousCursors.length > 0) && (
        <div className="flex justify-center mt-6">
          <div className="flex items-center gap-2">
            <Button size="lg" variant="bordered" onPress={goToPreviousPage} isDisabled={previousCursors.length === 0}>
              Previous
            </Button>
            <Button size="lg" variant="bordered" onPress={goToNextPage} isDisabled={!pageInfo?.hasNextPage}>
              Next
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

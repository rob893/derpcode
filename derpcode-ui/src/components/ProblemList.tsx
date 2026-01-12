import { useNavigate } from 'react-router';
import { useState, useMemo, useCallback } from 'react';
import { Card, CardBody, Chip, Button, Spinner, Divider, Select, SelectItem, Input, Tooltip } from '@heroui/react';
import {
  FunnelIcon,
  ArrowPathIcon,
  MagnifyingGlassIcon,
  StarIcon as StarIconOutline,
  CheckCircleIcon,
  ClockIcon
} from '@heroicons/react/24/outline';
import { StarIcon as StarIconSolid } from '@heroicons/react/24/solid';
import { ProblemDifficulty, ProblemOrderBy, OrderByDirection } from '../types/models';
import { ApiErrorDisplay } from './ApiErrorDisplay';
import {
  useProblemsLimitedPaginated,
  useProblemsCount,
  useAllTags,
  useFavoriteProblemForUser,
  useUnfavoriteProblemForUser
} from '../hooks/api';
import { useAuth } from '../hooks/useAuth';
import { hasAdminRole } from '../utils/auth';
import { useDebounce } from '../hooks/useDebounce';

export const ProblemList = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const isAdmin = hasAdminRole(user);

  const [pageSize, setPageSize] = useState(5);

  // State for filters and pagination
  const [selectedDifficulties, setSelectedDifficulties] = useState<Set<string>>(new Set());
  const [selectedTags, setSelectedTags] = useState<Set<string>>(new Set());
  const [favoriteFilter, setFavoriteFilter] = useState<'any' | 'true' | 'false'>('any');
  const [attemptedFilter, setAttemptedFilter] = useState<'any' | 'true' | 'false'>('any');
  const [passedFilter, setPassedFilter] = useState<'any' | 'true' | 'false'>('any');
  const [showFilters, setShowFilters] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [sortBy, setSortBy] = useState<ProblemOrderBy>(ProblemOrderBy.Difficulty);
  const [sortDirection, setSortDirection] = useState<OrderByDirection>(OrderByDirection.Ascending);
  const [cursor, setCursor] = useState<string | undefined>(undefined);
  const [previousCursors, setPreviousCursors] = useState<string[]>([]);

  // Fetch all tags using react-query
  const { data: allTags = [], error: tagsError } = useAllTags();

  // Debounce search query to avoid too many API calls
  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  // Build query parameters
  const queryParams = useMemo(() => {
    const isFavorite = favoriteFilter === 'any' ? undefined : favoriteFilter === 'true';
    const hasAttempted = attemptedFilter === 'any' ? undefined : attemptedFilter === 'true';
    const hasPassed = passedFilter === 'any' ? undefined : passedFilter === 'true';

    return {
      first: pageSize,
      after: cursor,
      includeUnpublished: isAdmin,
      searchTerm: debouncedSearchQuery.trim() || undefined,
      difficulties: Array.from(selectedDifficulties) as ProblemDifficulty[],
      tags: Array.from(selectedTags),
      orderBy: sortBy,
      orderByDirection: sortDirection,

      ...(isAuthenticated
        ? {
            isFavorite,
            hasAttempted,
            hasPassed
          }
        : {})
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
    isAuthenticated,
    favoriteFilter,
    attemptedFilter,
    passedFilter
  ]);

  const { data, isLoading, error, isFetching } = useProblemsLimitedPaginated(queryParams);
  const { data: problemsCount } = useProblemsCount();
  const favoriteProblem = useFavoriteProblemForUser(user?.id || 0);
  const unfavoriteProblem = useUnfavoriteProblemForUser(user?.id || 0);

  const isFavoriteMutationInFlight = favoriteProblem.isPending || unfavoriteProblem.isPending;

  // Memoize problems to avoid dependency issues in other useMemo hooks
  const problems = useMemo(() => data?.problems || [], [data?.problems]);
  const pageInfo = data?.pageInfo;
  const totalCount = problemsCount ?? 0;

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
    setFavoriteFilter('any');
    setAttemptedFilter('any');
    setPassedFilter('any');
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

  const handleFavoriteFilterChange = useCallback((keys: any) => {
    const selected = Array.from(keys)[0] as 'any' | 'true' | 'false' | undefined;
    setFavoriteFilter(selected ?? 'any');
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  const handleAttemptedFilterChange = useCallback((keys: any) => {
    const selected = Array.from(keys)[0] as 'any' | 'true' | 'false' | undefined;
    setAttemptedFilter(selected ?? 'any');
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  const handlePassedFilterChange = useCallback((keys: any) => {
    const selected = Array.from(keys)[0] as 'any' | 'true' | 'false' | undefined;
    setPassedFilter(selected ?? 'any');
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  const handleSearchChange = useCallback((value: string) => {
    setSearchQuery(value);
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  const handlePageSizeChange = useCallback((newSize: number) => {
    setPageSize(newSize);
    setCursor(undefined);
    setPreviousCursors([]);
  }, []);

  // Get current sort string for select component
  const currentSortString = useMemo(() => {
    const direction = sortDirection === OrderByDirection.Descending ? 'desc' : 'asc';
    return `${sortBy}-${direction}`;
  }, [sortBy, sortDirection]);

  // Select random problem
  const selectRandomProblem = useCallback(() => {
    if (problems.length > 0) {
      const randomIndex = Math.floor(Math.random() * problems.length);
      const randomProblem = problems[randomIndex];
      navigate(`/problems/${randomProblem.id}`);
    }
  }, [problems, navigate]);

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

  const formatStatusDate = (isoString: string) => {
    const date = new Date(isoString);
    if (Number.isNaN(date.getTime())) {
      return isoString;
    }
    return date.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
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

      {/* Tags loading error banner */}
      {tagsError && (
        <div className="bg-warning-50 border-l-4 border-warning p-4 rounded">
          <p className="text-sm text-warning-800">Unable to load tags for filtering. Tag filter may be incomplete.</p>
        </div>
      )}

      {/* Action buttons row with search bar in center */}
      <div className="flex justify-between items-center gap-4">
        <div className="flex items-center gap-4">
          <Button
            color="secondary"
            variant="ghost"
            size="md"
            onPress={selectRandomProblem}
            isDisabled={problems.length === 0}
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
              endContent={isFetching ? <Spinner size="sm" color="primary" /> : undefined}
              className="w-full"
              aria-label="Search problems"
              isClearable
              onClear={() => handleSearchChange('')}
            />
          </div>
        </div>

        <div className="flex items-center gap-3 shrink-0">
          {/* Page size selector */}
          <div className="flex items-center gap-2">
            <span className="text-md text-default-600">Per page:</span>
            <Select
              selectedKeys={[pageSize.toString()]}
              onSelectionChange={keys => {
                const selected = Array.from(keys)[0] as string | undefined;
                const parsed = selected ? Number.parseInt(selected, 10) : Number.NaN;
                if (Number.isFinite(parsed)) {
                  handlePageSizeChange(parsed);
                }
              }}
              className="w-24"
              variant="bordered"
              size="md"
              aria-label="Problems per page"
              name="problem-page-size"
            >
              <SelectItem key="5">5</SelectItem>
              <SelectItem key="10">10</SelectItem>
              <SelectItem key="25">25</SelectItem>
              <SelectItem key="50">50</SelectItem>
            </Select>
          </div>

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
                        <SelectItem key={tag.name}>{tag.name}</SelectItem>
                      ))}
                    </Select>
                  </label>
                </div>
              </div>

              {isAuthenticated && (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
                  <div className="space-y-2">
                    <label className="text-md font-medium text-foreground">
                      Favorites
                      <Select
                        selectedKeys={[favoriteFilter]}
                        onSelectionChange={handleFavoriteFilterChange}
                        className="w-full"
                        variant="bordered"
                        size="md"
                        aria-label="Filter by favorites"
                        name="problem-filter-favorite"
                      >
                        <SelectItem key="any">Any</SelectItem>
                        <SelectItem key="true">Only favorites</SelectItem>
                        <SelectItem key="false">Exclude favorites</SelectItem>
                      </Select>
                    </label>
                  </div>

                  <div className="space-y-2">
                    <label className="text-md font-medium text-foreground">
                      Attempted
                      <Select
                        selectedKeys={[attemptedFilter]}
                        onSelectionChange={handleAttemptedFilterChange}
                        className="w-full"
                        variant="bordered"
                        size="md"
                        aria-label="Filter by attempted"
                        name="problem-filter-attempted"
                      >
                        <SelectItem key="any">Any</SelectItem>
                        <SelectItem key="true">Attempted</SelectItem>
                        <SelectItem key="false">Not attempted</SelectItem>
                      </Select>
                    </label>
                  </div>

                  <div className="space-y-2">
                    <label className="text-md font-medium text-foreground">
                      Passed
                      <Select
                        selectedKeys={[passedFilter]}
                        onSelectionChange={handlePassedFilterChange}
                        className="w-full"
                        variant="bordered"
                        size="md"
                        aria-label="Filter by passed"
                        name="problem-filter-passed"
                      >
                        <SelectItem key="any">Any</SelectItem>
                        <SelectItem key="true">Passed</SelectItem>
                        <SelectItem key="false">Not passed</SelectItem>
                      </Select>
                    </label>
                  </div>
                </div>
              )}
            </div>

            {/* Clear Filters Button */}
            <div className="flex items-end">
              <Button
                variant="ghost"
                color="default"
                size="md"
                onPress={clearFilters}
                isDisabled={
                  searchQuery.trim() === '' &&
                  selectedDifficulties.size === 0 &&
                  selectedTags.size === 0 &&
                  (!isAuthenticated ||
                    (favoriteFilter === 'any' && attemptedFilter === 'any' && passedFilter === 'any'))
                }
              >
                Clear Filters
              </Button>
            </div>
          </div>

          {/* Active Filters Display */}
          {(searchQuery.trim() !== '' ||
            selectedDifficulties.size > 0 ||
            selectedTags.size > 0 ||
            (isAuthenticated && (favoriteFilter !== 'any' || attemptedFilter !== 'any' || passedFilter !== 'any'))) && (
            <div className="space-y-2">
              <div className="text-sm text-default-600">Active filters (showing {problems.length} results):</div>
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

                {isAuthenticated && favoriteFilter !== 'any' && (
                  <Chip color="warning" variant="flat" size="sm" onClose={() => setFavoriteFilter('any')}>
                    Favorites: {favoriteFilter === 'true' ? 'Only' : 'Exclude'}
                  </Chip>
                )}
                {isAuthenticated && attemptedFilter !== 'any' && (
                  <Chip color="default" variant="flat" size="sm" onClose={() => setAttemptedFilter('any')}>
                    Attempted: {attemptedFilter === 'true' ? 'Yes' : 'No'}
                  </Chip>
                )}
                {isAuthenticated && passedFilter !== 'any' && (
                  <Chip color="success" variant="flat" size="sm" onClose={() => setPassedFilter('any')}>
                    Passed: {passedFilter === 'true' ? 'Yes' : 'No'}
                  </Chip>
                )}
              </div>
            </div>
          )}
        </div>
      )}

      {!showFilters && <Divider />}

      {/* Pagination and Results Info */}
      <div className="flex justify-between items-center">
        <div className="text-sm text-default-600">
          Showing {problems.length} problems {totalCount ? `of ${totalCount} total` : ''}
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
        {problems.length === 0 ? (
          <Card className="p-8">
            <CardBody className="text-center">
              <div className="text-lg text-default-600 mb-2">No problems found</div>
              <div className="text-sm text-default-500">
                {searchQuery.trim() !== '' ||
                selectedDifficulties.size > 0 ||
                selectedTags.size > 0 ||
                (isAuthenticated && (favoriteFilter !== 'any' || attemptedFilter !== 'any' || passedFilter !== 'any'))
                  ? 'Try adjusting your search or filters'
                  : 'No problems available yet'}
              </div>
            </CardBody>
          </Card>
        ) : (
          problems.map(problem => (
            <div
              key={problem.id}
              role="button"
              tabIndex={0}
              onClick={() => navigate(`/problems/${problem.id}`)}
              onKeyDown={e => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  navigate(`/problems/${problem.id}`);
                }
              }}
              className="outline-none"
            >
              <Card isHoverable className="transition-all duration-200 hover:scale-[1.02] cursor-pointer">
                <CardBody className="p-6">
                  <div className="flex justify-between items-start mb-3">
                    <h3 className="text-xl font-semibold text-foreground hover:text-primary transition-colors">
                      {problem.name}
                    </h3>
                    <div className="flex items-center gap-2">
                      {isAuthenticated && (
                        <>
                          {(problem.lastPassedSubmissionDate || problem.lastSubmissionDate) && (
                            <Tooltip
                              content={
                                problem.lastPassedSubmissionDate
                                  ? `Last passed on ${formatStatusDate(problem.lastPassedSubmissionDate)}`
                                  : `Last attempted on ${formatStatusDate(problem.lastSubmissionDate!)}`
                              }
                              placement="bottom"
                            >
                              <div
                                className={
                                  problem.lastPassedSubmissionDate
                                    ? 'text-success'
                                    : 'text-default-500 dark:text-default-400'
                                }
                                aria-label={
                                  problem.lastPassedSubmissionDate
                                    ? `Last passed on ${formatStatusDate(problem.lastPassedSubmissionDate)}`
                                    : `Last attempted on ${formatStatusDate(problem.lastSubmissionDate!)}`
                                }
                              >
                                {problem.lastPassedSubmissionDate ? (
                                  <CheckCircleIcon className="h-5 w-5" />
                                ) : (
                                  <ClockIcon className="h-5 w-5" />
                                )}
                              </div>
                            </Tooltip>
                          )}

                          <Tooltip content={problem.isFavorite ? 'Unfavorite' : 'Favorite'} placement="bottom">
                            <Button
                              isIconOnly
                              size="sm"
                              variant="light"
                              color={problem.isFavorite ? 'warning' : 'default'}
                              aria-label={problem.isFavorite ? 'Unfavorite problem' : 'Favorite problem'}
                              data-testid={`favorite-toggle-${problem.id}`}
                              isDisabled={isFavoriteMutationInFlight}
                              onClick={e => e.stopPropagation()}
                              onPress={async () => {
                                if (!user) {
                                  return;
                                }

                                if (problem.isFavorite) {
                                  await unfavoriteProblem.mutateAsync(problem.id);
                                } else {
                                  await favoriteProblem.mutateAsync(problem.id);
                                }
                              }}
                            >
                              {problem.isFavorite ? (
                                <StarIconSolid className="h-5 w-5" />
                              ) : (
                                <StarIconOutline className="h-5 w-5" />
                              )}
                            </Button>
                          </Tooltip>
                        </>
                      )}
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
            </div>
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

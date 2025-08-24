import { useNavigate } from 'react-router';
import { useState, useMemo } from 'react';
import { Card, CardBody, Chip, Button, Spinner, Divider, Select, SelectItem, Input } from '@heroui/react';
import { FunnelIcon, ArrowPathIcon, MagnifyingGlassIcon } from '@heroicons/react/24/outline';
import { ProblemDifficulty } from '../types/models';
import { ApiErrorDisplay } from './ApiErrorDisplay';
import { useProblemsLimited } from '../hooks/api';
import { useAuth } from '../hooks/useAuth';
import { hasAdminRole } from '../utils/auth';

export const ProblemList = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const isAdmin = hasAdminRole(user);
  const {
    data: problems = [],
    isLoading,
    error
  } = useProblemsLimited({
    includeUnpublished: isAdmin
  });

  // Filter state
  const [selectedDifficulties, setSelectedDifficulties] = useState<Set<string>>(new Set());
  const [selectedTags, setSelectedTags] = useState<Set<string>>(new Set());
  const [showFilters, setShowFilters] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [sortBy, setSortBy] = useState<'difficulty-asc' | 'difficulty-desc' | 'name-asc' | 'name-desc'>(
    'difficulty-asc'
  );

  // Get all unique tags from problems
  const allTags = useMemo(() => {
    const tagSet = new Set<string>();
    problems.forEach(problem => {
      problem.tags?.forEach(tag => tagSet.add(tag.name));
    });
    return Array.from(tagSet).sort();
  }, [problems]);

  // Get difficulty sort order
  const getDifficultyOrder = (difficulty: ProblemDifficulty): number => {
    switch (difficulty) {
      case ProblemDifficulty.VeryEasy:
        return 1;
      case ProblemDifficulty.Easy:
        return 2;
      case ProblemDifficulty.Medium:
        return 3;
      case ProblemDifficulty.Hard:
        return 4;
      case ProblemDifficulty.VeryHard:
        return 5;
      default:
        return 6;
    }
  };

  // Filter and sort problems
  const filteredProblems = useMemo(() => {
    const filtered = problems.filter(problem => {
      // Check search query
      const searchMatch =
        searchQuery.trim() === '' ||
        problem.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        problem.tags?.some(tag => tag.name.toLowerCase().includes(searchQuery.toLowerCase()));

      // Check difficulty filter
      const difficultyMatch = selectedDifficulties.size === 0 || selectedDifficulties.has(problem.difficulty);

      // Check tags filter
      const tagsMatch = selectedTags.size === 0 || problem.tags?.some(tag => selectedTags.has(tag.name));

      return searchMatch && difficultyMatch && tagsMatch;
    });

    // Sort the filtered problems
    return filtered.sort((a, b) => {
      switch (sortBy) {
        case 'difficulty-asc':
          return getDifficultyOrder(a.difficulty) - getDifficultyOrder(b.difficulty);
        case 'difficulty-desc':
          return getDifficultyOrder(b.difficulty) - getDifficultyOrder(a.difficulty);
        case 'name-asc':
          return a.name.localeCompare(b.name);
        case 'name-desc':
          return b.name.localeCompare(a.name);
        default:
          return 0;
      }
    });
  }, [problems, searchQuery, selectedDifficulties, selectedTags, sortBy]);

  // Clear all filters
  const clearFilters = () => {
    setSearchQuery('');
    setSelectedDifficulties(new Set());
    setSelectedTags(new Set());
  };

  // Select random problem
  const selectRandomProblem = () => {
    if (filteredProblems.length > 0) {
      const randomIndex = Math.floor(Math.random() * filteredProblems.length);
      const randomProblem = filteredProblems[randomIndex];
      navigate(`/problems/${randomProblem.id}`);
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
              onChange={e => setSearchQuery(e.target.value)}
              variant="bordered"
              size="md"
              startContent={<MagnifyingGlassIcon className="w-4 h-4 text-default-400" />}
              className="w-full"
              aria-label="Search problems"
              isClearable
              onClear={() => setSearchQuery('')}
            />
          </div>
        </div>

        <div className="flex items-center gap-3 shrink-0">
          {/* Sort selector */}
          <div className="flex items-center gap-2">
            <span className="text-md text-default-600">Sort by:</span>
            <Select
              selectedKeys={[sortBy]}
              onSelectionChange={keys => setSortBy(Array.from(keys)[0] as typeof sortBy)}
              className="w-40"
              variant="bordered"
              size="md"
              aria-label="Sort problems by"
              name="problem-sort"
            >
              <SelectItem key="difficulty-asc">Difficulty (Easy → Hard)</SelectItem>
              <SelectItem key="difficulty-desc">Difficulty (Hard → Easy)</SelectItem>
              <SelectItem key="name-asc">Name (A → Z)</SelectItem>
              <SelectItem key="name-desc">Name (Z → A)</SelectItem>
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
                      onSelectionChange={keys => setSelectedDifficulties(new Set(Array.from(keys).map(String)))}
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
                      onSelectionChange={keys => setSelectedTags(new Set(Array.from(keys).map(String)))}
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
                Active filters ({filteredProblems.length} of {problems.length} problems):
              </div>
              <div className="flex flex-wrap gap-2">
                {searchQuery.trim() !== '' && (
                  <Chip color="default" variant="flat" size="sm" onClose={() => setSearchQuery('')}>
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
                    {problem.tags.map((tag, index) => (
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
    </div>
  );
};

using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;

namespace DerpCode.API.Data.Repositories;

public sealed class ProblemSubmissionRepository(DataContext context) : Repository<ProblemSubmission, CursorPaginationQueryParameters>(context), IProblemSubmissionRepository
{ }
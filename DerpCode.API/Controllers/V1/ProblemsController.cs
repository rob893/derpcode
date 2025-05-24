using System;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Models;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Services;
using DerpCode.API.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using DerpCode.API.Models.Responses.Pagination;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Requests;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
public class ProblemsController : ServiceControllerBase
{
    private readonly ILogger<ProblemsController> logger;
    private readonly ICodeExecutionService codeExecutionService;
    private readonly IProblemRepository problemRepository;
    private readonly IDriverTemplateRepository driverTemplateRepository;

    public ProblemsController(
        ILogger<ProblemsController> logger,
        ICorrelationIdService correlationIdService,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository,
        IDriverTemplateRepository driverTemplateRepository)
            : base(correlationIdService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        this.driverTemplateRepository = driverTemplateRepository ?? throw new ArgumentNullException(nameof(driverTemplateRepository));
    }

    [HttpGet("problems", Name = nameof(GetProblemsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<ProblemDto>>> GetProblemsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var problems = await this.problemRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        var response = problems.Select(ProblemDto.FromEntity).ToCursorPaginatedResponse(searchParams);
        return this.Ok(response);
    }

    [HttpGet("problems/{id}", Name = nameof(GetProblemAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProblemDto>> GetProblemAsync([FromRoute] int id)
    {
        var problem = await this.problemRepository.GetByIdAsync(id, track: false, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"Problem with ID {id} not found");
        }

        return this.Ok(ProblemDto.FromEntity(problem));
    }

    [HttpGet("driverTemplates", Name = nameof(GetDriverTemplatesAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedResponse<DriverTemplateDto>>> GetDriverTemplatesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var templates = await this.driverTemplateRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        var response = templates.Select(DriverTemplateDto.FromEntity).ToCursorPaginatedResponse(searchParams);
        return this.Ok(response);
    }

    [HttpPost("problems", Name = nameof(CreateProblemAsync))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProblemDto>> CreateProblemAsync([FromBody] CreateProblemRequest problem)
    {
        if (problem == null)
        {
            return this.BadRequest("Problem cannot be null");
        }

        var newProblem = problem.ToEntity();
        // ToDo: For tags, find all tags with the same name and use them instead of creating new ones to avoid duplicates.

        this.problemRepository.Add(newProblem);
        await this.problemRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        return this.CreatedAtRoute(nameof(GetProblemAsync), new { id = newProblem.Id }, ProblemDto.FromEntity(newProblem));
    }

    [HttpPost("problems/{id}/submissions", Name = nameof(SubmitSolutionAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SubmissionResult>> SubmitSolutionAsync([FromRoute] int id, [FromBody] SubmissionRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserCode))
        {
            return this.BadRequest("User code and language are required");
        }

        var problem = await this.problemRepository.GetByIdAsync(id, track: false, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return this.NotFound($"Problem with ID {id} not found");
        }

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == request.Language);
        if (driver == null)
        {
            return this.BadRequest($"No driver template found for language {request.Language}");
        }

        try
        {
            var result = await this.codeExecutionService.RunCodeAsync(request.UserCode, request.Language, problem, this.HttpContext.RequestAborted);
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code");
            return this.InternalServerError();
        }
    }
}

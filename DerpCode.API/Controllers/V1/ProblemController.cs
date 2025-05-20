using System;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Models;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Services;
using DerpCode.API.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using DerpCode.API.Core;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[AllowAnonymous]
public class ProblemController : ControllerBase
{
    private readonly ILogger<ProblemController> logger;
    private readonly ICodeExecutionService codeExecutionService;
    private readonly IProblemRepository problemRepository;
    private readonly IDriverTemplateRepository driverTemplateRepository;

    public ProblemController(
        ILogger<ProblemController> logger,
        ICodeExecutionService codeExecutionService,
        IProblemRepository problemRepository,
        IDriverTemplateRepository driverTemplateRepository)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
        this.problemRepository = problemRepository ?? throw new ArgumentNullException(nameof(problemRepository));
        this.driverTemplateRepository = driverTemplateRepository ?? throw new ArgumentNullException(nameof(driverTemplateRepository));
    }

    [HttpGet("problems", Name = nameof(GetProblemsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedList<Problem, int>>> GetProblemsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var problems = await this.problemRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        return this.Ok(problems.Select(MapProblem));
    }

    [HttpGet("problems/{id}", Name = nameof(GetProblemAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Problem>> GetProblemAsync([FromRoute] int id)
    {
        var problem = await this.problemRepository.GetByIdAsync(id, track: false, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return NotFound(new { error = "Problem not found" });
        }

        return Ok(MapProblem(problem));
    }

    [HttpGet("driverTemplates", Name = nameof(GetDriverTemplatesAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CursorPaginatedList<DriverTemplate, int>>> GetDriverTemplatesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
    {
        var templates = await this.driverTemplateRepository.SearchAsync(searchParams, track: false, this.HttpContext.RequestAborted);
        return this.Ok(templates);
    }

    [HttpPost("problems", Name = nameof(CreateProblemAsync))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Problem>> CreateProblemAsync([FromBody] Problem problem)
    {
        if (problem == null)
        {
            return this.BadRequest("Problem cannot be null");
        }

        if (string.IsNullOrEmpty(problem.Name) ||
            string.IsNullOrEmpty(problem.Description) ||
            string.IsNullOrEmpty(problem.Difficulty) ||
            problem.ExpectedOutput == null ||
            problem.Tags == null ||
            problem.Input == null ||
            problem.Drivers == null)
        {
            return this.BadRequest("Invalid problem data");
        }

        // Check if problem with this ID already exists
        var existingProblem = await this.problemRepository.GetByIdAsync(problem.Id, track: true, this.HttpContext.RequestAborted);

        if (existingProblem != null)
        {
            return this.Conflict(new { error = "Problem with this ID already exists" });
        }

        this.problemRepository.Add(problem);
        await this.problemRepository.SaveChangesAsync(this.HttpContext.RequestAborted);

        return this.CreatedAtAction(nameof(GetProblemAsync), new { id = problem.Id }, MapProblem(problem));
    }

    [HttpPost("problems/{id}/submissions", Name = nameof(SubmitSolutionAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<SubmissionResult>> SubmitSolutionAsync([FromRoute] int id, [FromBody] SubmissionRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserCode))
        {
            return BadRequest(new { error = "User code and language are required" });
        }

        var problem = await this.problemRepository.GetByIdAsync(id, track: false, this.HttpContext.RequestAborted);

        if (problem == null)
        {
            return NotFound(new { error = "Problem not found" });
        }

        var driver = problem.Drivers.FirstOrDefault(d => d.Language == request.Language);
        if (driver == null)
        {
            return BadRequest(new { error = $"No driver found for language: {request.Language}" });
        }

        try
        {
            var result = await this.codeExecutionService.RunCodeAsync(request.UserCode, request.Language, problem, this.HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error executing code");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    private static Problem MapProblem(Problem problem)
    {
        return new Problem
        {
            Id = problem.Id,
            Name = problem.Name,
            Description = problem.Description,
            Difficulty = problem.Difficulty,
            ExpectedOutput = problem.ExpectedOutput,
            Tags = problem.Tags,
            Input = problem.Input,
            Drivers = [.. problem.Drivers.Select(d => new ProblemDriver
            {
                Id = d.Id,
                Language = d.Language,
                Image = d.Image,
                UITemplate = d.UITemplate,
                DriverCode = string.Empty
            })]
        };
    }
}

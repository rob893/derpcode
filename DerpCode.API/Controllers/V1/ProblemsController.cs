using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DerpCode.API.Models;
using DerpCode.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}")]
[ApiVersion("1.0")]
[AllowAnonymous]
public class ProblemsController : ControllerBase
{
    private readonly ILogger<ProblemsController> logger;

    private readonly ICodeExecutionService codeExecutionService;

    private static readonly List<Problem> problems = [];

    private static readonly List<DriverTemplate> driverTemplates = Data.DriverTemplateData.Templates;

    public ProblemsController(ILogger<ProblemsController> logger, ICodeExecutionService codeExecutionService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.codeExecutionService = codeExecutionService ?? throw new ArgumentNullException(nameof(codeExecutionService));
    }

    [HttpGet("problems")]
    public ActionResult<IEnumerable<Problem>> GetProblems()
    {
        return this.Ok(problems.Select(MapProblem));
    }

    [HttpGet("problems/{id}")]
    public ActionResult<Problem> GetProblem([FromRoute] string id)
    {
        var problem = problems.FirstOrDefault(p => p.Id == id);

        if (problem == null)
        {
            return NotFound(new { error = "Problem not found" });
        }

        return Ok(MapProblem(problem));
    }

    [HttpGet("driverTemplates")]
    public ActionResult<IEnumerable<DriverTemplate>> GetDriverTemplates()
    {
        return this.Ok(driverTemplates);
    }

    [HttpPost("problems")]
    public ActionResult<Problem> CreateProblem([FromBody] Problem problem)
    {
        if (problem == null)
        {
            return this.BadRequest("Problem cannot be null");
        }

        if (string.IsNullOrEmpty(problem.Id) ||
            string.IsNullOrEmpty(problem.Name) ||
            string.IsNullOrEmpty(problem.Description) ||
            string.IsNullOrEmpty(problem.Difficulty) ||
            problem.ExpectedOutput == null ||
            problem.Tags == null ||
            problem.Input == null ||
            problem.Drivers == null)
        {
            return this.BadRequest("Invalid problem data");
        }

        if (problems.Any(p => p.Id == problem.Id))
        {
            return this.Conflict(new { error = "Problem with this ID already exists" });
        }

        problems.Add(problem);

        return this.CreatedAtAction(nameof(GetProblem), new { id = problem.Id }, MapProblem(problem));
    }

    [HttpPost("problems/{id}/submissions")]
    public async Task<ActionResult<SubmissionResult>> SubmitSolutionAsync([FromRoute] string id, [FromBody] SubmissionRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserCode))
        {
            return BadRequest(new { error = "User code and language are required" });
        }

        var problem = problems.FirstOrDefault(p => p.Id == id);

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
            var result = await this.codeExecutionService.RunCodeAsync(request.UserCode, request.Language, problem);
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
                UiTemplate = d.UiTemplate,
                DriverCode = string.Empty
            })]
        };
    }
}

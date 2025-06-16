namespace DerpCode.API.Services.Core;

public interface ICorrelationIdService
{
    string CorrelationId { get; set; }
}
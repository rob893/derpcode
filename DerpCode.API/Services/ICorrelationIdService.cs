namespace DerpCode.API.Services;

public interface ICorrelationIdService
{
    string CorrelationId { get; set; }
}
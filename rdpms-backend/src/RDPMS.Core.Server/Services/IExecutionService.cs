using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Services;

public interface IExecutionService
{
    Task<ExecutionSummaryDTO> RegisterFinishedAsync(ExecutionFinishedRequestDTO request);
    Task<List<ExecutionSummaryDTO>> GetAsync(Guid? ancestorOf = null, Guid? childOf = null);
    Task<ExecutionSummaryDTO> GetByIdAsync(Guid id);
}

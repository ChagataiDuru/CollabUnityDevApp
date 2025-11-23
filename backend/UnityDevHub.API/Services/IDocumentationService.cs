using UnityDevHub.API.Models.Documentation;

namespace UnityDevHub.API.Services
{
    public interface IDocumentationService
    {
        Task<IEnumerable<SearchResultDto>> SearchUnityDocsAsync(string query, Guid userId);
        Task<IEnumerable<PinnedDocDto>> GetPinnedDocsAsync(Guid projectId);
        Task<PinnedDocDto> PinDocAsync(Guid projectId, Guid userId, CreatePinnedDocDto dto);
        Task UnpinDocAsync(Guid id, Guid userId);
        Task<IEnumerable<SearchHistoryDto>> GetSearchHistoryAsync(Guid userId);
    }
}

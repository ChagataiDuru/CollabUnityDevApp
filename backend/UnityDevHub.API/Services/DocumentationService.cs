using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.Documentation;

namespace UnityDevHub.API.Services
{
    public class DocumentationService : IDocumentationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DocumentationService> _logger;

        public DocumentationService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<DocumentationService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IEnumerable<SearchResultDto>> SearchUnityDocsAsync(string query, Guid userId)
        {
            // Log search history
            var history = new SearchHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Query = query,
                SearchedAt = DateTime.UtcNow
            };
            _context.SearchHistory.Add(history);
            await _context.SaveChangesAsync();

            // Check cache
            string cacheKey = $"search_unity_{query.ToLowerInvariant()}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<SearchResultDto>? cachedResults))
            {
                return cachedResults!;
            }

            // Call Google Custom Search API
            var apiKey = _configuration["GoogleSearch:ApiKey"];
            var cx = _configuration["GoogleSearch:SearchEngineId"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(cx))
            {
                _logger.LogWarning("Google Search API Key or CX not configured.");
                return new List<SearchResultDto>();
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={cx}&q={Uri.EscapeDataString(query)}";

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);
                    var items = json.RootElement.GetProperty("items");

                    var results = new List<SearchResultDto>();
                    foreach (var item in items.EnumerateArray())
                    {
                        results.Add(new SearchResultDto
                        {
                            Title = item.GetProperty("title").GetString() ?? "",
                            Link = item.GetProperty("link").GetString() ?? "",
                            Snippet = item.GetProperty("snippet").GetString() ?? ""
                        });
                    }

                    // Cache for 24 hours
                    _cache.Set(cacheKey, results, TimeSpan.FromHours(24));
                    return results;
                }
                else
                {
                    _logger.LogError($"Google Search API failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Google Search API");
            }

            return new List<SearchResultDto>();
        }

        public async Task<IEnumerable<PinnedDocDto>> GetPinnedDocsAsync(Guid projectId)
        {
            return await _context.UnityDocReferences
                .Include(d => d.SavedBy)
                .Where(d => d.ProjectId == projectId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new PinnedDocDto
                {
                    Id = d.Id,
                    ProjectId = d.ProjectId,
                    Title = d.Title,
                    Url = d.Url,
                    Description = d.Description,
                    Notes = d.Notes,
                    SavedById = d.SavedById,
                    SavedByName = d.SavedBy != null ? (d.SavedBy.DisplayName ?? d.SavedBy.Username) : "Unknown",
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<PinnedDocDto> PinDocAsync(Guid projectId, Guid userId, CreatePinnedDocDto dto)
        {
            var doc = new UnityDocReference
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = dto.Title,
                Url = dto.Url,
                Description = dto.Description,
                Notes = dto.Notes,
                SavedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UnityDocReferences.Add(doc);
            await _context.SaveChangesAsync();

            // Load user for DTO
            await _context.Entry(doc).Reference(d => d.SavedBy).LoadAsync();

            return new PinnedDocDto
            {
                Id = doc.Id,
                ProjectId = doc.ProjectId,
                Title = doc.Title,
                Url = doc.Url,
                Description = doc.Description,
                Notes = doc.Notes,
                SavedById = doc.SavedById,
                SavedByName = doc.SavedBy != null ? (doc.SavedBy.DisplayName ?? doc.SavedBy.Username) : "Unknown",
                CreatedAt = doc.CreatedAt
            };
        }

        public async Task UnpinDocAsync(Guid id, Guid userId)
        {
            var doc = await _context.UnityDocReferences.FindAsync(id);
            if (doc != null)
            {
                // Optional: Check if user has permission to delete (e.g., only creator or admin)
                // For now, allow any member to unpin
                _context.UnityDocReferences.Remove(doc);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SearchHistoryDto>> GetSearchHistoryAsync(Guid userId)
        {
            return await _context.SearchHistory
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.SearchedAt)
                .Take(10) // Limit to last 10
                .Select(h => new SearchHistoryDto
                {
                    Id = h.Id,
                    Query = h.Query,
                    SearchedAt = h.SearchedAt
                })
                .ToListAsync();
        }
    }
}

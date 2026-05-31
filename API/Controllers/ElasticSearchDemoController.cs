using DAL.ElasticSearch;
using DTO.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for <see cref="IElasticSearchService{T}" /> typed on
///     <see cref="UserToListDto" />. Walks the index lifecycle (exists / create / delete), single
///     and bulk ingestion, and a field-scoped search. The connected index name is supplied per
///     call so you can experiment without touching the configured default.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class ElasticSearchDemoController(IElasticSearchService<UserToListDto> search) : ControllerBase
{
    /// <summary>Probes whether <paramref name="indexName" /> already exists in the cluster.</summary>
    [HttpGet("index/{indexName}/exists")]
    public async Task<IActionResult> IndexExists(string indexName)
    {
        var exists = await search.IndexExistsAsync(indexName);
        return Ok(new { indexName, exists });
    }

    /// <summary>Creates <paramref name="indexName" /> with the default mapping for <see cref="UserToListDto" />.</summary>
    [HttpPost("index/{indexName}")]
    public async Task<IActionResult> CreateIndex(string indexName)
    {
        var created = await search.CreateIndexAsync(indexName);
        return created
            ? Ok(new { indexName, note = "Index created." })
            : Conflict(new { indexName, note = "Index could not be created (it may already exist)." });
    }

    /// <summary>Deletes <paramref name="indexName" /> entirely — destructive.</summary>
    [HttpDelete("index/{indexName}")]
    public async Task<IActionResult> DeleteIndex(string indexName)
    {
        var deleted = await search.DeleteIndexAsync(indexName);
        return deleted
            ? Ok(new { indexName, note = "Index deleted." })
            : NotFound(new { indexName, note = "Index could not be deleted (it may not exist)." });
    }

    /// <summary>Indexes a single fabricated <see cref="UserToListDto" /> document.</summary>
    [HttpPost("index/{indexName}/documents")]
    public async Task<IActionResult> AddDocument(string indexName, [FromQuery] string username = "demo-user",
        [FromQuery] string email = "demo@example.com")
    {
        var doc = new UserToListDto(
            Guid.NewGuid(),
            username,
            email,
            "+1-555-0100",
            null,
            null);

        var ok = await search.AddToIndexAsync(doc, indexName);
        return ok
            ? Ok(new { indexName, doc, note = "Document indexed." })
            : StatusCode(StatusCodes.Status502BadGateway, new { indexName, note = "Index call failed." });
    }

    /// <summary>Bulk-indexes <paramref name="count" /> fabricated users in one request.</summary>
    [HttpPost("index/{indexName}/documents/bulk")]
    public async Task<IActionResult> AddDocumentsBulk(string indexName, [FromQuery] int count = 5)
    {
        var docs = Enumerable.Range(1, count).Select(i => new UserToListDto(
            Guid.NewGuid(),
            $"bulk-user-{i:D3}",
            $"bulk-user-{i:D3}@example.com",
            $"+1-555-{i:D4}",
            null,
            null)).ToList();

        var ok = await search.AddRangeToIndexAsync(docs, indexName);
        return ok
            ? Ok(new { indexName, count = docs.Count, note = "Bulk-indexed." })
            : StatusCode(StatusCodes.Status502BadGateway, new { indexName, note = "Bulk index call failed." });
    }

    /// <summary>
    ///     Searches <paramref name="indexName" /> by matching <paramref name="query" /> against
    ///     the chosen <paramref name="field" /> — <c>username</c> or <c>email</c>.
    /// </summary>
    [HttpGet("index/{indexName}/search")]
    public async Task<IActionResult> Search(string indexName, [FromQuery] string query,
        [FromQuery] string field = "username")
    {
        var hits = field.ToLowerInvariant() switch
        {
            "email" => await search.SearchDocumentsAsync(u => u.Email, query),
            "username" => await search.SearchDocumentsAsync(u => u.Username, query),
            _ => null
        };

        if (hits is null)
            return BadRequest(new { note = "field must be 'username' or 'email'." });

        return Ok(new { indexName, field, query, results = hits });
    }
}
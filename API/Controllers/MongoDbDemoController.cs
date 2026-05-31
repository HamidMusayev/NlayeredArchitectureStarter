using DAL.MongoDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for <see cref="IMongoDbService" />. Walks the CRUD surface plus
///     filter-by-expression and a raw <c>IMongoCollection&lt;T&gt;</c> escape hatch. All endpoints
///     operate on a single <c>notes</c> collection in the configured database.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class MongoDbDemoController(IMongoDbService mongo) : ControllerBase
{
    private const string Collection = "notes";

    /// <summary>Lists every document in the <c>notes</c> collection.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notes = await mongo.GetAll<NoteDocument>(Collection);
        return Ok(notes);
    }

    /// <summary>Fetches a single note by its string id (e.g. the one returned from <c>POST</c>).</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var note = await mongo.GetById<NoteDocument>(Collection, id);
        return note is null ? NotFound() : Ok(note);
    }

    /// <summary>Filter-by-expression — driver translates the predicate to a Mongo query.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string author)
    {
        var notes = await mongo.GetByFilter<NoteDocument>(Collection, n => n.Author == author);
        return Ok(notes);
    }

    /// <summary>Inserts a note. The id is generated server-side as a GUID string.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NoteRequest body)
    {
        var doc = new NoteDocument
        {
            Id = Guid.NewGuid().ToString(),
            Title = body.Title,
            Author = body.Author,
            Body = body.Body,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await mongo.Insert(Collection, doc);
        return CreatedAtAction(nameof(GetById), new { id = doc.Id }, doc);
    }

    /// <summary>Replaces the document at <paramref name="id" /> with the supplied payload.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] NoteRequest body)
    {
        var existing = await mongo.GetById<NoteDocument>(Collection, id);
        if (existing is null) return NotFound();

        existing.Title = body.Title;
        existing.Author = body.Author;
        existing.Body = body.Body;

        await mongo.Update(Collection, id, existing);
        return Ok(existing);
    }

    /// <summary>Deletes the note at <paramref name="id" />.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await mongo.Delete<NoteDocument>(Collection, id);
        return NoContent();
    }

    /// <summary>
    ///     Raw-collection escape hatch — when the convenience helpers don't fit (aggregations,
    ///     bulk writes, projections), reach for <see cref="IMongoDbService.GetCollection{T}" />
    ///     and drive the driver directly. Returns the total document count.
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> Count()
    {
        var collection = mongo.GetCollection<NoteDocument>(Collection);
        var count = await collection.CountDocumentsAsync(Builders<NoteDocument>.Filter.Empty);
        return Ok(new { collection = Collection, count });
    }

    public sealed record NoteRequest(string Title, string Author, string Body);

    public sealed class NoteDocument
    {
        [BsonId] public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
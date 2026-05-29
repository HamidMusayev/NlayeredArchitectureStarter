using DAL.EntityFramework.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Profiling;

namespace API.Controllers;

/// <summary>
///     Demonstration endpoints for MiniProfiler instrumentation. Hit any endpoint then open
///     <c>/profiler/results-index</c> to see the timing tree. <c>MiniProfiler.Current.Step</c>
///     wraps a synchronous block; <c>StepAsync</c> wraps an awaited block. <c>CustomTiming</c>
///     is for non-step events like outbound HTTP or cache hits. Steps nest naturally — open
///     one inside another to get a hierarchy in the UI.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class MiniProfilerDemoController(IRoleRepository roleRepository) : ControllerBase
{
    /// <summary>
    ///     Three sequential steps showing the basic shape: each <c>using</c> block becomes a row
    ///     in the profiler tree with its own elapsed time.
    /// </summary>
    [HttpGet("sequential-steps")]
    public async Task<IActionResult> SequentialSteps(CancellationToken ct)
    {
        var profiler = MiniProfiler.Current;

        using (profiler.Step("Step 1: warm-up"))
        {
            await Task.Delay(50, ct);
        }

        using (profiler.Step("Step 2: load roles from DB"))
        {
            // The AddEntityFramework() hook auto-captures the SQL emitted here.
            _ = await roleRepository.GetListAsync();
        }

        using (profiler.Step("Step 3: post-processing"))
        {
            await Task.Delay(30, ct);
        }

        return Ok(new { note = "Open /profiler/results-index to inspect the timing tree." });
    }

    /// <summary>
    ///     Nested steps — the inner <c>using</c> blocks render indented under their parent,
    ///     so you can see how an outer phase breaks down into sub-phases.
    /// </summary>
    [HttpGet("nested-steps")]
    public async Task<IActionResult> NestedSteps(CancellationToken ct)
    {
        var profiler = MiniProfiler.Current;

        using (profiler.Step("Outer: build response"))
        {
            using (profiler.Step("Inner: fetch data"))
            {
                _ = await roleRepository.GetListAsync();
            }

            using (profiler.Step("Inner: transform"))
            {
                await Task.Delay(20, ct);

                using (profiler.Step("Inner.Inner: serialize"))
                {
                    await Task.Delay(10, ct);
                }
            }
        }

        return Ok(new { note = "Inspect the nested tree at /profiler/results-index." });
    }

    /// <summary>
    ///     <c>CustomTiming</c> records a non-step event (e.g. outbound HTTP, Redis GET) with its
    ///     own category column in the UI.
    /// </summary>
    [HttpGet("custom-timing")]
    public async Task<IActionResult> CustomTiming(CancellationToken ct)
    {
        var profiler = MiniProfiler.Current;

        using (profiler.Step("Call external API"))
        using (profiler.CustomTiming("http", "GET https://example.com/widgets"))
        {
            await Task.Delay(75, ct);
        }

        using (profiler.Step("Read from cache"))
        using (profiler.CustomTiming("redis", "GET role:list"))
        {
            await Task.Delay(5, ct);
        }

        return Ok(new { note = "Custom timings appear in their own columns at /profiler/results-index." });
    }
}

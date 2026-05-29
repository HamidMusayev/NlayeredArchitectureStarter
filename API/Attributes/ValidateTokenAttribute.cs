using API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Attributes;

/// <summary>
///     Convenience attribute that wires <see cref="ValidateTokenFilter" /> onto an action or
///     controller without requiring manual <c>TypeFilterAttribute</c> boilerplate. Apply to any
///     endpoint that should verify the application-level token store beyond the JWT bearer check.
/// </summary>
public class ValidateTokenAttribute() : TypeFilterAttribute(typeof(ValidateTokenFilter));
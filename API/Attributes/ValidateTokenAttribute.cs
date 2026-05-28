using API.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Attributes;

public class ValidateTokenAttribute() : TypeFilterAttribute(typeof(ValidateTokenFilter));
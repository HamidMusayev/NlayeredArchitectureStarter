using CORE.Localization;
using DTO.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Filters;

/// <summary>
///     Action filter that short-circuits the request with a 400 Bad Request and an
///     <c>ErrorDataResult&lt;ModelStateDictionary&gt;</c> body when the model state is invalid.
///     Registered globally so every controller action is covered without per-action attributes.
/// </summary>
public class ModelValidatorActionFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var result = new ErrorDataResult<ModelStateDictionary>(context.ModelState, Messages.InvalidModel.Translate());
        context.Result = new BadRequestObjectResult(result);
    }
}
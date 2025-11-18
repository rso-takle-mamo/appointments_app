using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Runtime.CompilerServices;
using UserService.Api.Exceptions;
using UserService.Api.Models;

namespace UserService.Api.Filters;

public class ModelValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var validationErrors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(error => new ValidationError
                {
                    Field = x.Key == "" ? "Request body" : x.Key,
                    Message = error.ErrorMessage
                }))
                .ToList();

            throw new ValidationException(validationErrors);
        }

        base.OnActionExecuting(context);
    }
}
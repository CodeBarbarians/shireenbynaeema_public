namespace Server
{
    using System.ComponentModel.DataAnnotations;

    using Microsoft.AspNetCore.Mvc.Filters;

    /// <summary>
    /// Provides an action filter that validates the model state before an action method executes and throws a
    /// ValidationException if the model state is invalid.
    /// </summary>
    /// <remarks>Use this filter to enforce model validation in ASP.NET Core controllers. If the incoming
    /// request contains invalid model data, the filter prevents the action method from executing and returns detailed
    /// validation error messages in the exception. This filter is typically applied globally or at the controller level
    /// to ensure consistent model validation across actions.</remarks>
    public class ValidateModelFilter : IActionFilter
    {
        /// <summary>
        /// Called before the action method executes. Validates the model state of the incoming request.
        /// If the model state is invalid, it collects all errors and throws a ValidationException.
        /// </summary>
        /// <param name="context">
        /// The ActionExecutingContext containing information about the current request and action.
        /// </param>
        /// <exception cref="ValidationException">
        /// Thrown when the model state is invalid, including all validation error messages.
        /// </exception>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                            .Where(e => e.Value?.Errors?.Count > 0)
                            .SelectMany(kvp => kvp.Value?.Errors?.Select(err => $"{kvp.Key}: {err.ErrorMessage}") ?? Enumerable.Empty<string>())
                            .ToList();
                throw new ValidationException(string.Join(" | ", errors));
            }
        }

        /// <summary>
        /// Called after the action method executes. This implementation is intentionally left empty
        /// because model validation is only performed before the action executes.
        /// </summary>
        /// <param name="context">
        /// The ActionExecutedContext containing information about the executed action and its result.
        /// </param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
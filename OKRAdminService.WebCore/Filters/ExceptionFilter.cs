using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OKRAdminService.Services.Contracts;
using System;
using System.Net;

namespace OKRAdminService.WebCore.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IUserService userService;

        public ExceptionFilter(IUserService user) : base()
        {
            userService = user;
        }

        public override void OnException(ExceptionContext context)
        {
            var controller = string.Empty;
            var action = string.Empty;

            var statusCode = HttpStatusCode.InternalServerError;
            if (context.Exception is DataNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)statusCode;

            if (context.RouteData != null)
            {
                action = context.RouteData.Values["action"].ToString();
                controller = context.RouteData.Values["controller"].ToString();
            }

            context.Result = new JsonResult(new
            {
                error = new[] { context.Exception.Message },
                stackTrace = context.Exception.StackTrace
            });
            context.ExceptionHandled = true;
            userService.SaveLog(controller, action, context.Exception.ToString() + "InnerException" + context.Exception.InnerException);

        }
    }
}

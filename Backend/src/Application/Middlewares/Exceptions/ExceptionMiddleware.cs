using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using IERIC.SumariosIERIC.Application.Attributes;

namespace IERIC.SumariosIERIC.Application.Middlewares

{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        private readonly IDictionary<Type, IResultError> _types;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env, IDictionary<Type, IResultError> types)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _types = types;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {

                Type exceptionType = ex.GetType();

                IResultError errorResult = null;

                foreach (KeyValuePair<Type, IResultError> type in _types)
                {
                    if (type.Key.IsAssignableFrom(exceptionType))
                    {
                        errorResult = type.Value;
                        errorResult.Message = ex.Message;
                    }
                }

                if (errorResult == null)
                {
                    //Error no controlado
                    _logger.LogError(ex, ex.Message);
                    errorResult = new BasicResultError();
                }
                errorResult.Map(ex);
                context.Response.StatusCode = errorResult.StatusCode;

                if (!_env.IsDevelopment())
                {
                    foreach (PropertyInfo prop in errorResult.GetType().GetProperties())
                    {
                        var NotShow = prop.GetCustomAttribute(typeof(NotShowInProductionAttribute));
                        if (NotShow != null)
                        {
                            prop.SetValue(errorResult, null);
                        }

                    }
                }

                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(errorResult.ToJson());
            }
        }
    }

}
﻿using Audit.Core;
using MediatR;
using Shoping.Application.Common.Attributes;
using Shoping.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Shoping.Application.Common.Behaviours;

public class AuditLogsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditLogsBehavior<TRequest, TResponse>> _logger;
    private readonly IConfiguration _config;

    public AuditLogsBehavior(
        ICurrentUserService currentUserService,
        ILogger<AuditLogsBehavior<TRequest, TResponse>> logger,
        IConfiguration config)
    {
        _currentUserService = currentUserService;
        _logger = logger;
        _config = config;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("{RequetsName}: {@User} with request {@Request}",
            typeof(TRequest).Name, _currentUserService.User.Id, request);


        IAuditScope? scope = null;

        var auditLogAttributes = request.GetType().GetCustomAttributes<AuditLogAttribute>();
        if (auditLogAttributes.Any() && _config.GetValue<bool>("AuditLogs:Enabled"))
        {
            // El IRequest cuenta con el atributo [AuditLog] para ser auditado
            scope = AuditScope.Create(_ => _
                .EventType(typeof(TRequest).Name)
                .ExtraFields(new
                {
                    _currentUserService.User,
                    Request = request
                }));
        }

        var result = await next();


        if (scope is not null)
        {
            await scope.DisposeAsync();
        }

        return result;
    }
}

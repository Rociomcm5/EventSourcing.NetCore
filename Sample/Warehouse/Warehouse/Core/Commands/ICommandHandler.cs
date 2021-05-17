﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Warehouse.Core.Commands
{
    public interface ICommandHandler<in T>
    {
        ValueTask<CommandResult> Handle(T command, CancellationToken token);
    }

    public record CommandResult
    {
        public object? Result { get; }

        private CommandResult(object? result = null)
            => Result = result;

        public static CommandResult None => new();

        public static CommandResult Of(object result) => new(result);
    }

    public static class CommandHandlerConfiguration
    {
        public static IServiceCollection AddCommandHandler<T, TCommandHandler>(
            this IServiceCollection services,
            Func<IServiceProvider, TCommandHandler>? configure = null
        ) where TCommandHandler: class, ICommandHandler<T>
        {

            if (configure == null)
            {
                services.AddTransient<TCommandHandler, TCommandHandler>();
                services.AddTransient<ICommandHandler<T>, TCommandHandler>();
            }
            else
            {
                services.AddTransient<TCommandHandler, TCommandHandler>(configure);
                services.AddTransient<ICommandHandler<T>, TCommandHandler>(configure);
            }

            return services;
        }

        public static ICommandHandler<T> GetCommandHandler<T>(this HttpContext context)
            => context.RequestServices.GetRequiredService<ICommandHandler<T>>();


        public static ValueTask<CommandResult> SendCommand<T>(this HttpContext context, T command)
            => context.GetCommandHandler<T>()
                .Handle(command, context.RequestAborted);
    }
}

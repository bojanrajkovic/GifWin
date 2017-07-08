using System;
using System.Collections.Generic;

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GifWin.Core.Services
{
    [PublicAPI]
    public sealed class ServiceContainer
    {
        // ReSharper disable once InconsistentNaming
        static readonly Lazy<ServiceContainer> instance = new Lazy<ServiceContainer>();

        public static ServiceContainer Instance => instance.Value;

        readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public void RegisterService<T>(T service) => services.Add(typeof(T), service);

        public T GetOptionalService<T>() =>
            services.TryGetValue(typeof(T), out var maybeService) ? (T)maybeService : default(T);

        public T GetRequiredService<T>() =>
            (T)services[typeof(T)];

        public ILogger GetLogger(string categoryName) =>
            GetOptionalService<ILoggerFactory>()?.CreateLogger(categoryName);

        public ILogger<T> GetLogger<T>() =>
            GetOptionalService<ILoggerFactory>()?.CreateLogger<T>();
    }
}

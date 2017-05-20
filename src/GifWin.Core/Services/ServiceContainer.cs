using System;
using System.Collections.Generic;

namespace GifWin.Core.Services
{
    public class ServiceContainer
    {
        static readonly Lazy<ServiceContainer> instance = new Lazy<ServiceContainer>();
        public static ServiceContainer Instance => instance.Value;

        readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public void RegisterService<T>(T service) => services.Add(typeof(T), service);

        public T GetOptionalService<T>() =>
            services.TryGetValue(typeof(T), out var maybeService) ? (T)maybeService : default(T);

        public T GetRequiredService<T>() =>
            (T)services[typeof(T)];
    }
}

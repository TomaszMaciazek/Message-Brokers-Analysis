using Producer.RabbitMQ.API.Services;

namespace Producer.RabbitMQ.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IPublisherService, PublisherService>();
            return services;
        }
    }
}

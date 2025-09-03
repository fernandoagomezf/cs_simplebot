
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleBot.Infrastructure.Data;
using SimpleBot.Infrastructure.Repositories;
using SimpleBot.Infrastructure.Services;
using SimpleBot.Application.Bots;
using SimpleBot.Application.Bots.Adapters;
using SimpleBot.Application.Bots.Dialogs;
using Microsoft.EntityFrameworkCore;

namespace SimpleBot.Application;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }


    public void ConfigureServices(IServiceCollection services) {
        services.AddHttpClient().AddControllers().AddNewtonsoftJson(options => {
            options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
        });


        services.AddDbContext<BotContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("BotContext"))
        );

        services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
        services.AddScoped<IIntentRepository, IntentRepository>();
        services.AddScoped<ITextClassifier, NaiveBayesClassifier>();
        services.AddSingleton<IStorage, MemoryStorage>();
        services.AddSingleton<ConversationState>(sp => {
            var storage = sp.GetRequiredService<IStorage>();
            return new ConversationState(storage);
        });

        services.AddSingleton<UserState>(sp => {
            var storage = sp.GetRequiredService<IStorage>();
            return new UserState(storage);
        });

        services.AddMemoryCache();
        services.AddSingleton<SupportDialog>();
        services.AddTransient<IBot, EchoBot>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }

        app.UseDefaultFiles()
            .UseStaticFiles()
            .UseWebSockets()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
    }
}

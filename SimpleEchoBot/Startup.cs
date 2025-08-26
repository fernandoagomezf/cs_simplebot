
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleBot.Adapters;
using SimpleBot.Services;

namespace SimpleBot;

public class Startup {
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }


    public void ConfigureServices(IServiceCollection services) {
        services.AddHttpClient().AddControllers().AddNewtonsoftJson(options => {
            options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
        });

        services.AddSingleton<RuleBasedClassifier>();

        services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

        services.AddSingleton<IStorage, MemoryStorage>();

        services.AddSingleton<ConversationState>(sp => {
            var storage = sp.GetRequiredService<IStorage>();
            return new ConversationState(storage);
        });

        services.AddSingleton<UserState>(sp => {
            var storage = sp.GetRequiredService<IStorage>();
            return new UserState(storage);
        });


        services.AddSingleton<Dialogs.SupportDialog>();

        services.AddTransient<IBot, Bots.EchoBot>();


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

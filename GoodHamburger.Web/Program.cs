using GoodHamburger.Web.Components;
using GoodHamburger.Web.Integration;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace GoodHamburger.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.SectionName));
            builder.Services.AddHttpClient<GoodHamburgerApiClient>((serviceProvider, client) =>
            {
                ApiOptions options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

            var app = builder.Build();

            var supportedCultures = new[] { new CultureInfo("pt-BR"), new CultureInfo("en-US") };
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("pt-BR"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRequestLocalization(localizationOptions);
            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                if (IsPathTraversalAttempt(context.Request) || IsNonCanonicalNavigationPath(context.Request))
                {
                    context.Response.Redirect("/");
                    return;
                }

                await next();
            });

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

        private static bool IsPathTraversalAttempt(HttpRequest request)
        {
            if (ContainsTraversalPattern(request.Path.Value))
            {
                return true;
            }

            string? rawTarget = request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;
            return ContainsTraversalPattern(rawTarget);
        }

        private static bool IsNonCanonicalNavigationPath(HttpRequest request)
        {
            string? path = request.Path.Value;
            if (string.IsNullOrWhiteSpace(path) || path == "/")
            {
                return false;
            }

            if (path.StartsWith("/_", StringComparison.Ordinal) || path.Contains('.', StringComparison.Ordinal))
            {
                return false;
            }

            return path.Any(char.IsLetter) && path != path.ToLowerInvariant();
        }

        private static bool ContainsTraversalPattern(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string current = value;

            for (int i = 0; i < 2; i++)
            {
                if (HasTraversalToken(current))
                {
                    return true;
                }

                string decoded = Uri.UnescapeDataString(current);
                if (decoded == current)
                {
                    break;
                }

                current = decoded;
            }

            return HasTraversalToken(current);
        }

        private static bool HasTraversalToken(string value)
        {
            string normalized = value.Replace('\\', '/');
            return normalized.Contains("../", StringComparison.Ordinal) || normalized.Contains("/..", StringComparison.Ordinal);
        }
    }
}

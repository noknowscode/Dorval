using System.Diagnostics.CodeAnalysis;
using Uno.Resizetizer;

namespace Dorval;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }

    protected IHost? Host { get; private set; }

    [SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Uno.Extensions APIs are used in a way that is safe for trimming in this template context.")]
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            // Teaches navigation about Toolkit controls, including handing the
            // ExtendedSplashScreen the ILoadable that lets it stand down.
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning),
                    enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddSingleton<IDorvalCatalog, DorvalCatalog>()
                        .AddSingleton<SavedStore>()
                        .AddSingleton<EngagementStore>()
                        .AddSingleton<UserProfile>()
                        .AddSingleton<AppFilters>()
                        .AddSingleton<ToastService>()
                        .AddSingleton<ShellChrome>();

#if DEBUG
                    RegisterStartScreenData(services);
#endif
                })
                .UseNavigation(RegisterRoutes)
            );

        MainWindow = builder.Window;

#if DEBUG
        if (Environment.GetEnvironmentVariable("APP_NO_HOTDESIGN") != "1")
        {
            MainWindow.UseStudio();
        }
#endif
        MainWindow.SetWindowIcon();
        SizeToPhone(MainWindow);

        Host = await builder.NavigateAsync<Shell>();

        // Navigation hands the ShellViewModel to the frame it creates rather than to the
        // Shell itself, so the app-level toast layer above that frame has nothing to bind
        // to. Seed it from the root, which the Shell inherits.
        if (MainWindow.Content is FrameworkElement root)
        {
            root.DataContext = Host.Services.GetRequiredService<ShellViewModel>();
        }

#if DEBUG
        NavProbe();
#endif
    }

    /// <summary>
    /// This is a phone app. On desktop, give it a phone-shaped window rather than
    /// stretching a 375-wide layout across a monitor.
    /// </summary>
    private static void SizeToPhone(Window window)
    {
#if HAS_UNO_SKIA || __SKIA__
        window.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 400, Height = 860 });
#endif
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<OnboardingPage, OnboardingViewModel>(),
            new ViewMap<TodayPage, TodayViewModel>(),
            new ViewMap<DiscoverPage, DiscoverViewModel>(),
            new ViewMap<PlacesPage, PlacesViewModel>(),
            new ViewMap<CommunityPage, CommunityViewModel>(),
            new ViewMap<SavedPage, SavedViewModel>(),
            new ViewMap<ReminderPage, ReminderViewModel>(),
            new ViewMap<SafetyPage, SafetyViewModel>(),
            new DataViewMap<EventDetailPage, EventDetailViewModel, DorvalEvent>(),
            new DataViewMap<PostPage, PostViewModel, CommunityPost>()
        );

        var start = StartRoute;

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new RouteMap("Today", View: views.FindByViewModel<TodayViewModel>(), IsDefault: start == "Today"),
                    new RouteMap("Onboarding", View: views.FindByViewModel<OnboardingViewModel>(), IsDefault: start == "Onboarding"),
                    new RouteMap("Discover", View: views.FindByViewModel<DiscoverViewModel>(), IsDefault: start == "Discover"),
                    new RouteMap("Places", View: views.FindByViewModel<PlacesViewModel>(), IsDefault: start == "Places"),
                    new RouteMap("Community", View: views.FindByViewModel<CommunityViewModel>(), IsDefault: start == "Community"),
                    new RouteMap("Saved", View: views.FindByViewModel<SavedViewModel>(), IsDefault: start == "Saved"),
                    new RouteMap("Event", View: views.FindByViewModel<EventDetailViewModel>(), IsDefault: start == "Event"),
                    new RouteMap("Reminder", View: views.FindByViewModel<ReminderViewModel>(), IsDefault: start == "Reminder"),
                    new RouteMap("Post", View: views.FindByViewModel<PostViewModel>(), IsDefault: start == "Post"),
                    new RouteMap("Safety", View: views.FindByViewModel<SafetyViewModel>(), IsDefault: start == "Safety"),
                ]
            )
        );
    }

    /// <summary>
    /// Which screen the app opens on: onboarding until it has been completed, Today
    /// after that. In Debug, DORVAL_START_ROUTE overrides it so a screen can be opened
    /// directly for a screenshot or a manual check.
    /// </summary>
    private static string StartRoute
    {
        get
        {
#if DEBUG
            if (DebugStartScreen is { Route.Length: > 0 } start)
            {
                return start.Route;
            }
#endif
            return UserProfile.IsOnboarded ? "Today" : "Onboarding";
        }
    }

#if DEBUG
    /// <summary>
    /// Presses a tab a few seconds after launch so a transition can be filmed without a
    /// pointer — synthesized input cannot reach an Uno desktop window from a background
    /// process. Driven by <c>tools/Probe-Transition.ps1</c>.
    /// </summary>
    /// <remarks>
    /// DORVAL_NAV_PROBE takes one or more tabs separated by commas, each optionally
    /// suffixed with a delay. The delays are cumulative, so "Places@4000,Saved@2000"
    /// presses Places at 4s and Saved at 6s. Nothing runs unless the variable is set.
    /// </remarks>
    private void NavProbe()
    {
        var value = Environment.GetEnvironmentVariable("DORVAL_NAV_PROBE");
        if (string.IsNullOrWhiteSpace(value) || Host is null)
        {
            return;
        }

        // The harness counts its lead-in from this marker, not from the window appearing:
        // a cold start can put ten seconds between the two, which mis-centres the film.
        ProbeTrace("ready");

        _ = Task.Run(async () =>
        {
            foreach (var step in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = step.Split('@', 2);
                var delay = parts.Length > 1 && int.TryParse(parts[1], out var ms) ? ms : 3000;
                var tab = parts[0].Trim();

                await Task.Delay(delay);

                MainWindow!.DispatcherQueue.TryEnqueue(async () =>
                {
                    var navigator = Host!.Services.GetRequiredService<ShellChrome>().DebugNavigator;
                    if (navigator is null)
                    {
                        ProbeTrace("probe has no navigator");
                        return;
                    }

                    await navigator.NavigateRouteAsync(this, $"-/{tab}");
                    ProbeTrace($"probe navigated to {tab}");
                });
            }
        });
    }

    /// <summary>Appends a marker to the file the transition harness watches.</summary>
    private static void ProbeTrace(string line)
    {
        try
        {
            System.IO.File.AppendAllText(
                System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dorval-route.log"),
                line + Environment.NewLine);
        }
        catch
        {
            // Tracing must never take the app down.
        }
    }

    /// <summary>Parses DORVAL_START_ROUTE, which is a route name with an optional item id: "Event:walk".</summary>
    private static (string Route, string? Id)? DebugStartScreen
    {
        get
        {
            var value = Environment.GetEnvironmentVariable("DORVAL_START_ROUTE");
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var parts = value.Split(':', 2);
            return (parts[0], parts.Length > 1 ? parts[1] : null);
        }
    }

    /// <summary>
    /// Event and post pages normally take their subject from the navigation request.
    /// When one is opened directly as the start screen there is no request, so the
    /// subject is registered for the view model to resolve instead.
    /// </summary>
    private static void RegisterStartScreenData(IServiceCollection services)
    {
        if (DebugStartScreen is not { } start)
        {
            return;
        }

        var catalog = new DorvalCatalog();

        switch (start.Route)
        {
            case "Event":
                services.AddSingleton(catalog.Event(start.Id ?? "cinema")!);
                break;
            case "Post":
                services.AddSingleton(catalog.Post(start.Id ?? "swim")!);
                break;
            default:
                // Every screen but onboarding assumes onboarding already happened.
                if (start.Route != "Onboarding")
                {
                    new UserProfile().CompleteOnboarding();
                }

                break;
        }
    }
#endif
}

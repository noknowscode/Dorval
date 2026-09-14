namespace Dorval.Services;

public interface IDorvalCatalog
{
    IReadOnlyList<DorvalEvent> Events { get; }

    IReadOnlyList<Place> Places { get; }

    IReadOnlyList<CommunityPost> Posts { get; }

    IReadOnlyList<HelpTile> HelpTiles { get; }

    IReadOnlyList<GuidelineRule> Guidelines { get; }

    DorvalEvent? Event(string id);

    Place? Place(string id);

    CommunityPost? Post(string id);

    IReadOnlyList<PostReply> RepliesFor(string postId);

    void AddReply(string postId, PostReply reply);
}

/// <summary>
/// In-memory city content. A real deployment would swap this for the municipal
/// events/places feeds behind the same interface.
/// </summary>
public sealed class DorvalCatalog : IDorvalCatalog
{
    private readonly Dictionary<string, List<PostReply>> _replies = new()
    {
        ["swim"] = [new("Marc D.", "Dorval Resident", "1h ago", "Surrey Centre has great instructors for that age group!")],
        ["pool"] = [new("Tom B.", "Dorval Resident", "1h ago", "Open! The maintenance closure was last week.")],
        ["bike"] = [],
        ["washer"] = [],
    };

    public IReadOnlyList<DorvalEvent> Events { get; } =
    [
        new("cinema", "Outdoor Family Cinema", "Sat, Aug 14", "Sunset", "Surrey Aquatic & Community Centre",
            IsFree: true, IsWeekend: true, IsToday: false, IsForKids: true,
            ["Family", "Outdoors", "Free"],
            ["Bring a chair or blanket.", "Weather dependent."]),
        new("skate", "Skatefest", "Saturday", "1:00 PM", "Dorval Skatepark",
            IsFree: true, IsWeekend: true, IsToday: false, IsForKids: true,
            ["Sports", "Outdoors", "Free"],
            ["Helmets required.", "All ages welcome."]),
        new("concert", "Summer Concert", "Saturday", "7:30 PM", "Pine Beach Park",
            IsFree: false, IsWeekend: true, IsToday: false, IsForKids: false,
            ["Music", "Outdoors"],
            ["Tickets $12 at the gate.", "Food trucks on site."]),
        new("dance", "Dance in the Park", "Sunday", "2:00 PM", "Millar Park",
            IsFree: true, IsWeekend: true, IsToday: false, IsForKids: true,
            ["Family", "Outdoors", "Free"],
            ["No experience needed.", "Bring water."]),
        new("market", "Solidarity Market", "Thu, Sep 3", "4:00 – 7:00 PM", "Surrey Centre",
            IsFree: true, IsWeekend: false, IsToday: true, IsForKids: false,
            ["Community", "Free"],
            ["Bring your own bags.", "Cash and card accepted."]),
        new("walk", "Guided Lakeshore Heritage Walk", "Sat, Sept 13", "10:00 AM", "Pine Beach Park",
            IsFree: true, IsWeekend: false, IsToday: false, IsForKids: false,
            ["Heritage", "Outdoors", "Free"],
            ["Registration required.", "About 90 minutes, 3 km."]),
        new("dropoff", "Environmental Drop-off", "Sat, Sept 6", "9:00 AM – 5:00 PM", "Ballantyne Park",
            IsFree: true, IsWeekend: false, IsToday: false, IsForKids: false,
            ["Environment", "Free"],
            ["Electronics, paint and batteries accepted."]),
        new("storytime", "Library Storytime", "Sat, Aug 16", "10:30 AM", "Dorval Library",
            IsFree: true, IsWeekend: false, IsToday: false, IsForKids: true,
            ["Kids", "Free"],
            ["Ages 2–5.", "Drop-in, no registration."]),
        new("celebrates", "Dorval Celebrates", "Sat, Aug 16", "12:00 PM", "Pine Beach Park",
            IsFree: true, IsWeekend: false, IsToday: false, IsForKids: false,
            ["Community", "Free"],
            ["Shuttle from Surrey Centre."]),
    ];

    public IReadOnlyList<Place> Places { get; } =
    [
        new("surrey", "Surrey Aquatic & Community Centre", "Community centre", "8 min away", "Open until 9:00 PM",
            ["Outdoor pool", "Splash pad", "Pickleball", "Community rooms"], ["Pools", "Sports"], 80, 62),
        new("library", "Dorval Library", "Library", "5 min away", "Open until 9:00 PM",
            ["Study rooms", "Kids corner", "Wi-Fi"], ["Culture"], 244, 52),
        new("pine", "Pine Beach Park", "Park", "12 min away", "Open 24 hours",
            ["Beach", "Playground", "Picnic tables"], ["Parks"], 60, 118),
        new("museum", "Dorval Museum of Local History", "Museum", "10 min away", "Open until 5:00 PM",
            ["Exhibits", "Heritage"], ["Culture"], 290, 104),
    ];

    public IReadOnlyList<CommunityPost> Posts { get; } =
    [
        new("pool", PostCategory.Question, "Is Walters pool open today?", "Julie R.", "Dorval Village", "2h ago",
            "Answered ✓ · 800 m away", 2,
            "The website says closed for maintenance but I saw people there this morning. Anyone know?"),
        new("bike", PostCategory.Free, "Kids bike · 16\"", "Marc D.", "Dorval Village", "2h ago",
            "Dorval Village · 2h ago", 3,
            "Our daughter outgrew it. Good condition, needs a little air in the tires. Pick up in Dorval Village."),
        new("washer", PostCategory.Help, "Looking to borrow a pressure washer", "Anne T.", "Surrey", "4h ago",
            "Surrey · 4h ago", 2,
            "Just for an afternoon to clean the deck. Happy to return the favour with a batch of cookies!"),
        new("swim", PostCategory.Question, "Best beginner swimming lessons for a 5 year old?", "Sarah L.",
            "Dorval Village", "3h ago", "Dorval Village · 3h ago", 12,
            "We're new to lessons and looking for recommendations in Dorval. Thanks!"),
    ];

    public IReadOnlyList<HelpTile> HelpTiles { get; } =
    [
        new("Ask a question", PostCategory.Question, "M21 12a8 8 0 0 1-8 8H5l-2 2V12a8 8 0 0 1 16 0zM10 9.5a2 2 0 1 1 2 2v1M12 15.5h.01"),
        new("Sell or give", PostCategory.Free, "M20.6 13.4l-7.2 7.2a2 2 0 0 1-2.8 0L3 13V3h10l7.6 7.6a2 2 0 0 1 0 2.8zM7.5 7.5h.01"),
        new("Ask for help", PostCategory.Help, "M12 21c-1-1-7-5-7-10a4 4 0 0 1 7-2.5A4 4 0 0 1 19 11c0 2-1 4-3 6M8 21h8"),
        new("Start a discussion", PostCategory.Sell, "M9 11a3 3 0 1 0 0-6 3 3 0 0 0 0 6zM17 12a3 3 0 1 0 0-6M3 20a6 6 0 0 1 12 0M15 20a5 5 0 0 1 6-4.9"),
    ];

    public IReadOnlyList<GuidelineRule> Guidelines { get; } =
    [
        new("Verified residents only", "M12 2l8 3v6c0 5-3.5 9.5-8 11-4.5-1.5-8-6-8-11V5zM9 12l2 2 4-4"),
        new("No anonymous accounts", "M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM4 21a8 8 0 0 1 16 0"),
        new("No personal attacks or hate speech", "M18 6L6 18M6 6l12 12"),
        new("No spam or commercial ads", "M4 4h16v12H4zM8 20h8M3 8h18"),
        new("Keep personal information private", "M6 11V8a6 6 0 0 1 12 0v3M5 11h14v10H5z"),
    ];

    public DorvalEvent? Event(string id) => Events.FirstOrDefault(e => e.Id == id);

    public Place? Place(string id) => Places.FirstOrDefault(p => p.Id == id);

    public CommunityPost? Post(string id) => Posts.FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<PostReply> RepliesFor(string postId) =>
        _replies.TryGetValue(postId, out var list) ? list : [];

    public void AddReply(string postId, PostReply reply)
    {
        if (!_replies.TryGetValue(postId, out var list))
        {
            list = [];
            _replies[postId] = list;
        }

        list.Add(reply);
    }
}

namespace EventEaseApp.Helpers;

// Resolves image src for venue and event listings. Returns the user-supplied URL
// when present and non-empty, otherwise a neutral local fallback that ships in
// wwwroot/images/. Centralises the rule so the same fallback is used wherever
// venue/event imagery is rendered.
public static class ImageHelper
{
    public const string VenueFallback = "/images/venue-fallback.jpg";
    public const string EventFallback = "/images/event-fallback.jpg";

    public static string ResolveVenueImage(string? imageUrl)
        => string.IsNullOrWhiteSpace(imageUrl) ? VenueFallback : imageUrl!;

    public static string ResolveEventImage(string? imageUrl)
        => string.IsNullOrWhiteSpace(imageUrl) ? EventFallback : imageUrl!;
}

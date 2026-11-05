namespace WardrobeAI.Services;

public static class ClothingItemEvents
{
    public static event EventHandler<ClothingItemAddedEventArgs>? ItemAdded;

    public static void RaiseItemAdded(int userId) => ItemAdded?.Invoke(null, new ClothingItemAddedEventArgs(userId));

    public static void Reset() => ItemAdded = null;
}

public sealed class ClothingItemAddedEventArgs : EventArgs
{
    public ClothingItemAddedEventArgs(int userId) => UserId = userId;

    public int UserId { get; }
}

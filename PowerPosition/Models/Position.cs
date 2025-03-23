namespace PowerPosition;

/// <summary>
/// Represents a power position with a specific date and volume.
/// </summary>
/// <param name="Date">The date and time of the position.</param>
/// <param name="Volume">The volume of the position.</param>
public record Position(DateTime Date, double Volume);

namespace SharpMod;

/// <summary>
/// Standard Amiga Protracker constants.
/// </summary>
public static class ModConstants
{
    /// <summary>
    /// C2SPD (samples/sec at C-2) for finetune values 0..15.
    /// Finetune 0..7 = positive, 8..15 = negative (-8..-1).
    /// </summary>
    public static readonly short[] FineTune =
    {
        8363, 8413, 8463, 8529, 8581, 8651, 8723, 8757,
        7895, 7941, 7985, 8046, 8107, 8169, 8232, 8280
    };
}

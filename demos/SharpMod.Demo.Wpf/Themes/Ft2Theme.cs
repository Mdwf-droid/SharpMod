using SkiaSharp;

namespace SharpMod.Demo.Wpf.Themes;

/// <summary>
/// Palette de couleurs et constantes visuelles FastTracker II.
/// </summary>
public static class Ft2Theme
{
    // ── Fond ──
    public static readonly SKColor Background      = new(0xFF0B0E14);
    public static readonly SKColor PanelBg         = new(0xFF131824);
    public static readonly SKColor PanelInset       = new(0xFF0A0D12);

    // ── Bordures ──
    public static readonly SKColor BorderLight     = new(0xFF2A3550);
    public static readonly SKColor BorderDark      = new(0xFF060810);

    // ── Pattern Editor ──
    public static readonly SKColor RowNumberColor  = new(0xFF4A5580);
    public static readonly SKColor CurrentRowBg    = new(0xFF1A2540);
    public static readonly SKColor NoteColor       = new(0xFF50D050);   // Vert
    public static readonly SKColor InstrumentColor = new(0xFFE0A030);   // Jaune/or
    public static readonly SKColor VolumeColor     = new(0xFF40C0E0);   // Cyan
    public static readonly SKColor EffectColor     = new(0xFFE04080);   // Rose
    public static readonly SKColor EffectDataColor = new(0xFFE04080);
    public static readonly SKColor DotColor        = new(0xFF252A3A);   // Points "vides"
    public static readonly SKColor SeparatorColor  = new(0xFF1A1F30);

    // ── Scopes ──
    public static readonly SKColor[] ScopeColors =
    [
        new(0xFF40B040), new(0xFF40A0D0), new(0xFFD0A040), new(0xFFD04080),
        new(0xFF8040D0), new(0xFF40D0A0), new(0xFFD06040), new(0xFF4080D0),
    ];
    public static readonly SKColor ScopeLine      = new(0x30304060);
    public static readonly SKColor ScopeBorder     = new(0xFF1A1F2E);

    // ── VU-meters ──
    public static readonly SKColor VuGreen         = new(0xFF40C040);
    public static readonly SKColor VuYellow        = new(0xFFC0C040);
    public static readonly SKColor VuRed           = new(0xFFC04040);
    public static readonly SKColor VuOff           = new(0xFF0E1218);

    // ── Transport ──
    public static readonly SKColor TitleColor      = new(0xFF8090B0);
    public static readonly SKColor ValueColor      = new(0xFFD0D8E8);
    public static readonly SKColor ButtonBg        = new(0xFF1A2040);
    public static readonly SKColor ButtonHover     = new(0xFF253060);

    // ── Dimensions ──
    public const float RowHeight       = 16f;
    public const float RowNumWidth     = 30f;
    public const float CellNoteWidth   = 36f;
    public const float CellInstWidth   = 24f;
    public const float CellVolWidth    = 24f;
    public const float CellFxWidth     = 16f;
    public const float CellFxDataWidth = 24f;
    public const float CellSepWidth    = 6f;

    public const float ScopeHeight    = 52f;
    public const float VuMeterWidth   = 6f;

    // Largeur totale d'une cellule de canal
    public static float CellWidth =>
        CellNoteWidth + CellInstWidth + CellVolWidth +
        CellFxWidth + CellFxDataWidth + CellSepWidth;

    // ── Police ──
    public static SKTypeface LoadFont()
    {
        // Charge la police embarquée
        using var stream = typeof(Ft2Theme).Assembly
            .GetManifestResourceStream("SharpMod.Demo.Wpf.Fonts.ShareTechMono-Regular.ttf");
        if (stream != null)
            return SKTypeface.FromStream(stream);
        return SKTypeface.FromFamilyName("Consolas", SKFontStyle.Normal);
    }

    public static SKPaint CreateTextPaint(SKColor color, float size = 12f, SKTypeface? typeface = null)
    {
        return new SKPaint
        {
            Color = color,
            IsAntialias = true,
            TextSize = size,
            Typeface = typeface ?? SKTypeface.FromFamilyName("Consolas"),
        };
    }
}

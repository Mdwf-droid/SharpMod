using SkiaSharp;

namespace SharpMod.Demo.Wpf.Themes;

/// <summary>
/// Palette de couleurs et constantes visuelles FastTracker II.
/// </summary>
public static class Ft2Theme
{
    
    public static SKColor RowNumberColor => new SKColor(0x50, 0x60, 0x80);
    public static SKColor NoteColor => new SKColor(0xD0, 0xD8, 0xE8);
    public static SKColor InstrumentColor => new SKColor(0xFF, 0xD0, 0x40);
    public static SKColor EffectColor => new SKColor(0x40, 0xC0, 0x40);
    public static SKColor DotColor => new SKColor(0x25, 0x30, 0x50);
    public static SKColor SeparatorColor => new SKColor(0x1A, 0x1F, 0x2E);
    public static SKColor ScopeBorder => new SKColor(0x1A, 0x1F, 0x2E);
    public static SKColor VuGreen => new SKColor(0x40, 0xC0, 0x40);
    public static SKColor VuYellow => new SKColor(0xC0, 0xC0, 0x40);
    public static SKColor VuRed => new SKColor(0xC0, 0x40, 0x40);
    public static SKColor VuOff => new SKColor(0x0E, 0x12, 0x18);

    // ── Fond ──
    public static readonly SKColor Background = new(0xFF0B0E14);
    public static readonly SKColor PanelBg = new(0xFF131824);
    public static readonly SKColor PanelInset = new(0xFF0A0D12);

    // ── Bordures ──
    public static readonly SKColor BorderLight = new(0xFF2A3550);
    public static readonly SKColor BorderDark = new(0xFF060810);

    // ── Pattern Editor ──
    //public static readonly SKColor RowNumberColor = new(0xFF4A5580);
    public static readonly SKColor CurrentRowBg = new(0xFF1A2540);
    //public static readonly SKColor NoteColor = new(0xFF50D050);   // Vert
    //public static readonly SKColor InstrumentColor = new(0xFFE0A030);   // Jaune/or
    public static readonly SKColor VolumeColor = new(0xFF40C0E0);   // Cyan
    //public static readonly SKColor EffectColor = new(0xFFE04080);   // Rose
    public static readonly SKColor EffectDataColor = new(0xFFE04080);
    //public static readonly SKColor DotColor = new(0xFF252A3A);   // Points "vides"
    //public static readonly SKColor SeparatorColor = new(0xFF1A1F30);

    // ── Scopes ──
    public static SKColor[] ScopeColors =>
    [
    new SKColor(0x40, 0xB0, 0x40), new SKColor(0x40, 0xA0, 0xD0),
    new SKColor(0xD0, 0xA0, 0x40), new SKColor(0xD0, 0x40, 0x80),
    new SKColor(0x80, 0x40, 0xD0), new SKColor(0x40, 0xD0, 0xA0),
    new SKColor(0xD0, 0x60, 0x40), new SKColor(0x40, 0x80, 0xD0),
    new SKColor(0x40, 0xB0, 0x40), new SKColor(0x40, 0xA0, 0xD0),
    new SKColor(0xD0, 0xA0, 0x40), new SKColor(0xD0, 0x40, 0x80),
    new SKColor(0x80, 0x40, 0xD0), new SKColor(0x40, 0xD0, 0xA0),
    new SKColor(0xD0, 0x60, 0x40), new SKColor(0x40, 0x80, 0xD0),
    ];

    public static readonly SKColor ScopeLine = new(0x30304060);
    //public static readonly SKColor ScopeBorder = new(0xFF1A1F2E);

    // ── VU-meters ──
   /* public static readonly SKColor VuGreen = new(0xFF40C040);
    public static readonly SKColor VuYellow = new(0xFFC0C040);
    public static readonly SKColor VuRed = new(0xFFC04040);
    public static readonly SKColor VuOff = new(0xFF0E1218);*/

    // ── Transport ──
    public static readonly SKColor TitleColor = new(0xFF8090B0);
    public static readonly SKColor ValueColor = new(0xFFD0D8E8);
    public static readonly SKColor ButtonBg = new(0xFF1A2040);
    public static readonly SKColor ButtonHover = new(0xFF253060);

    // ── Dimensions ──
    public static float RowNumWidth => 32f;     // largeur colonne numéro de row
    public static float CellWidth => 100f;      // largeur d'une colonne channel
    public static float CellNoteWidth => 32f;   // sous-colonne note (C-4)
    public static float CellInstWidth => 20f;   // sous-colonne instrument (0F)
    public static float RowHeight => 16f;        // hauteur d'une row



    public const float CellVolWidth = 24f;
    public const float CellFxWidth = 16f;
    public const float CellFxDataWidth = 24f;
    public const float CellSepWidth = 6f;

    public const float ScopeHeight = 52f;
    public const float VuMeterWidth = 6f;

    // Largeur totale d'une cellule de canal
    /* public static float CellWidth =>
         CellNoteWidth + CellInstWidth + CellVolWidth +
         CellFxWidth + CellFxDataWidth + CellSepWidth;*/

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



    public static SKPaint CreateTextPaint(SKColor color, float size)
    {
        return new SKPaint
        {
            Color = color,
            TextSize = size,
            Typeface = SKTypeface.FromFamilyName("Consolas",
                SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright),
            IsAntialias = true
        };
    }
}

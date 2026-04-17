namespace SharpMod.Song;

public class PatternCell
{
    public int Period { get; set; }
    public int? Note { get; set; }
    public int? Octave { get; set; }
    public int Instrument { get; set; }
    public int Effect { get; set; }
    public int EffectData { get; set; }

    private static readonly string[] NoteNames =
        { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    public override string ToString()
    {
        string noteStr = Note.HasValue && Note.Value >= 0 && Note.Value < 12
            ? NoteNames[Note.Value] : "--";
        string octStr = Octave.HasValue ? Octave.Value.ToString() : "-";
        string instStr = Instrument != 0 ? $"{Instrument:D2}" : "--";
        string fxStr = (Effect != 0 || EffectData != 0)
            ? $"{Effect:X2}" : "--";
        string fxDataStr = (Effect != 0 || EffectData != 0)
            ? $"{EffectData:X2}" : "--";

        return $"{noteStr}{octStr} {instStr} {fxStr}{fxDataStr}";
    }
}

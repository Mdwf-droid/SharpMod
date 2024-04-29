using System;
using System.ComponentModel;
using System.Text;


namespace SharpMod.Song
{

    public class PatternCell : INotifyPropertyChanged
    {
        static readonly string[] ReadableNotes = ["C ", "C#", "D ", "D#", "E ", "F ", "F#", "G ", "G#", "A ", "A#", "B "];

        public event PropertyChangedEventHandler PropertyChanged;

       
        private short _period;
        public short Period
        {
            get { return _period; }
            set
            {
                if (_period == value)
                    return;
                _period = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Period)));
            }
        }

        private int? _note;
        public int? Note
        {
            get { return _note; }
            set
            {
                if (_note == value)
                    return;
                _note = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Note)));
            }
        }

        private int? _octave;
        public int? Octave
        {
            get { return _octave; }
            set
            {
                if (_octave == value)
                    return;
                _octave = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Octave)));
            }
        }

        private short _instrument;
        public short Instrument
        {
            get { return _instrument; }
            set
            {
                if (_instrument == value)
                    return;
                _instrument = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Instrument)));
            }
        }

        private short _effect;
        public short Effect
        {
            get { return _effect; }
            set
            {
                if (_effect == value)
                    return;
                _effect = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Effect)));
            }
        }

        private short _effectData;
        public short EffectData
        {
            get { return _effectData; }
            set
            {
                if (_effectData == value)
                    return;
                _effectData = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(EffectData)));
            }
        }

        public PatternCell(Track parentTrack)
        {            
            parentTrack?.RegisterPatternCellEvent(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append((Note != null) ? ReadableNotes[(int)Note] : "--");
            sb.Append((Note != null) ? ((Octave + 1).ToString()) : "-");
            sb.Append(' ');
            sb.Append((Instrument != 0) ? String.Format("{0:00}", Instrument) : "--");
            sb.Append(' ');
            sb.Append(Effect != 0 ? String.Format("{0:X2}", Effect) : "--");
            sb.Append(Effect != 0 ? String.Format("{0:X2}", EffectData) : "--");
            return sb.ToString();
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}

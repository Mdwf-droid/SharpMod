using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace SharpMod.Wpf.UI.UserControls
{

    public class LedPresenter : ContentControl
    {
        internal static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(LedPresenter), new PropertyMetadata(ValueChanged));
        internal static readonly DependencyProperty MaskValueProperty = DependencyProperty.Register("MaskValue", typeof(string), typeof(LedPresenter), null);

        public string? Value { get { return (string)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }



        public string? MaskValue
        {
            get => GetValue(MaskValueProperty)?.ToString();
            set { SetValue(MaskValueProperty, value); }
        }

        [DefaultValue(10)]
        public int DigitCount { get; set; }

        [DefaultValue(TextAlignment.Left)]
        public TextAlignment Align { get; set; }

        public LedPresenter()
            : base()
        {
            DefaultStyleKey = typeof(LedPresenter);
            DigitCount = 10;
            Align = TextAlignment.Left;
        }

        public override void OnApplyTemplate()
        {
            Update(this, "");
            base.OnApplyTemplate();
        }

        private static void Update(LedPresenter? lp, string? val)
        {
            var toPrint = val;
            if (lp != null)
            {

                if (val?.Length > lp.DigitCount)
                {
                    toPrint = toPrint?.Substring(0, lp.DigitCount);
                }

                switch (lp.Align)
                {
                    case TextAlignment.Left:
                        toPrint = toPrint?.PadRight(lp.DigitCount);
                        break;

                    case TextAlignment.Right:
                        toPrint = toPrint?.PadLeft(lp.DigitCount);
                        break;
                    case TextAlignment.Center:
                        toPrint = toPrint?.PadLeft(lp.DigitCount / 2);
                        toPrint = toPrint?.PadRight(lp.DigitCount / 2);
                        break;
                }

                lp.SetValue(MaskValueProperty, new string('A', lp.DigitCount));
                lp.SetValue(ValueProperty, toPrint);
            }
        }

        private static void ValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var val = Convert.ToString(e.NewValue);

            LedPresenter? lp = o as LedPresenter;

            Update(lp, val);
        }
    }
}

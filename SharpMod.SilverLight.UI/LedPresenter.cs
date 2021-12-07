using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;


namespace SharpMod.SilverLight.UI
{

    public class LedPresenter : ContentControl
    {
        public static DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(String), typeof(LedPresenter), new PropertyMetadata(ValueChanged));
        public static DependencyProperty MaskValueProperty = DependencyProperty.Register("MaskValue", typeof(String), typeof(LedPresenter), null);

        public String Value { get { return (string)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

        TextBlock _lblPresenter;
        TextBlock _lblPresenterBack;

        public string MaskValue
        {
            get { return Convert.ToString(GetValue(MaskValueProperty)); }
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
            _lblPresenter = GetTemplateChild("lblPresenter") as TextBlock;
            _lblPresenterBack = GetTemplateChild("lblPresenterBack") as TextBlock;
            Update(this, "");
            base.OnApplyTemplate();
        }

        private static void Update(LedPresenter lp,string val)
        {
            String toPrint = val;
            if (lp != null)
            {
                
                if(val.Length > lp.DigitCount)
                {
                    toPrint = toPrint.Substring(0, lp.DigitCount);
                }

                switch (lp.Align)
                {
                    case TextAlignment.Left:
                       toPrint= toPrint.PadRight(lp.DigitCount);
                        break;

                    case TextAlignment.Right:
                        toPrint = toPrint.PadLeft(lp.DigitCount);
                        break;
                    case TextAlignment.Center:
                        toPrint = toPrint.PadLeft(lp.DigitCount / 2);
                        toPrint = toPrint.PadRight(lp.DigitCount / 2);
                        break;
                }

                lp.SetValue(MaskValueProperty, new string('A', lp.DigitCount));
                lp.SetValue(ValueProperty, toPrint);
            }
        }

        private static void ValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            String val = Convert.ToString(e.NewValue);
            
            LedPresenter lp = o as LedPresenter;

            Update(lp, val);
        }
    }
}

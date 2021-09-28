using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TagReporter.Views
{
    /// <summary>
    /// Interaction logic for TimeTextBox.xaml
    /// </summary>
    public partial class TimePicker : UserControl
    {
        public List<int> HourList { get; set; } = new List<int>();
        public List<int> MinuteList { get; set; } = new List<int>();

        public new int Height { get; set; }
        public new int Width { get; set; }

        public TimeSpan Value
        {
            get => (TimeSpan)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public int Hours
        {
            get => (int)GetValue(HoursProperty);
            set => SetValue(HoursProperty, value);
        }
        public int Minutes
        {
            get => (int)GetValue(MinutesProperty);
            set => SetValue(MinutesProperty, value);
        }

        public TimePicker()
        {
            for (int i = 0; i < 24; i++)
            {
                HourList.Add(i);
            }
            for (int i = 0; i < 60; i++)
            {
                MinuteList.Add(i);
            }
            InitializeComponent();

        }


        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimePicker),
            new PropertyMetadata(DateTime.Now.TimeOfDay, new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TimePicker;
            if (ctrl != null)
            {
                ctrl.Hours = ((TimeSpan)e.NewValue).Hours;
                ctrl.Minutes = ((TimeSpan)e.NewValue).Minutes;
            }
        }

        public static readonly DependencyProperty HoursProperty = DependencyProperty.Register("Hours", typeof(int), typeof(TimePicker), new PropertyMetadata(0, new PropertyChangedCallback(OnTimeChanged)));

        public static readonly DependencyProperty MinutesProperty = DependencyProperty.Register("Minutes", typeof(int), typeof(TimePicker), new PropertyMetadata(0, new PropertyChangedCallback(OnTimeChanged)));

        private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TimePicker;
            if (ctrl != null)
                ctrl.Value = new TimeSpan(ctrl.Hours, ctrl.Minutes, 0);
        }

    }
}

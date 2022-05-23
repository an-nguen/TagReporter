using System;
using System.Collections.Generic;
using System.Windows;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for TimeTextBox.xaml
/// </summary>
public partial class TimePicker
{
    public List<int> HourList { get; set; } = new();
    public List<int> MinuteList { get; set; } = new();

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
        for (var i = 0; i < 24; i++)
        {
            HourList.Add(i);
        }
        for (var i = 0; i < 60; i++)
        {
            MinuteList.Add(i);
        }
        InitializeComponent();

    }


    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimePicker),
        new PropertyMetadata(DateTime.Now.TimeOfDay, OnValueChanged));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePicker ctrl)
        {
            ctrl.Hours = ((TimeSpan)e.NewValue).Hours;
            ctrl.Minutes = ((TimeSpan)e.NewValue).Minutes;
        }
    }

    public static readonly DependencyProperty HoursProperty = DependencyProperty.Register("Hours", typeof(int), typeof(TimePicker), new PropertyMetadata(0, new PropertyChangedCallback(OnTimeChanged)));

    public static readonly DependencyProperty MinutesProperty = DependencyProperty.Register("Minutes", typeof(int), typeof(TimePicker), new PropertyMetadata(0, new PropertyChangedCallback(OnTimeChanged)));

    private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimePicker ctrl)
            ctrl.Value = new TimeSpan(ctrl.Hours, ctrl.Minutes, 0);
    }

}
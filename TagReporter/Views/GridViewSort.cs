using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TagReporter.Views;

public class GridViewSort
{
    #region Attached properties

    public static ICommand GetCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(CommandProperty);
    }

    public static void SetCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(CommandProperty, value);
    }

    // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(GridViewSort),
            new UIPropertyMetadata(
                null,
                (o, e) =>
                {
                    if (o is not ItemsControl listView) return;
                    if (GetAutoSort(listView)) return;
                    if (e.OldValue != null && e.NewValue == null)
                    {
                        listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                    }
                    if (e.OldValue == null && e.NewValue != null)
                    {
                        listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                    }
                }
            )
        );

    public static bool GetAutoSort(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoSortProperty);
    }

    public static void SetAutoSort(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoSortProperty, value);
    }

    // Using a DependencyProperty as the backing store for AutoSort.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty AutoSortProperty =
        DependencyProperty.RegisterAttached(
            "AutoSort",
            typeof(bool),
            typeof(GridViewSort),
            new UIPropertyMetadata(
                false,
                (o, e) =>
                {
                    if (o is not ListView listView) return;
                    if (GetCommand(listView) != null) return;
                    var oldValue = (bool)e.OldValue;
                    var newValue = (bool)e.NewValue;
                    switch (oldValue)
                    {
                        case true when !newValue:
                            listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                            break;
                        case false when newValue:
                            listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                            break;
                    }
                }
            )
        );

    public static string GetPropertyName(DependencyObject obj)
    {
        return (string)obj.GetValue(PropertyNameProperty);
    }

    public static void SetPropertyName(DependencyObject obj, string value)
    {
        obj.SetValue(PropertyNameProperty, value);
    }

    // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PropertyNameProperty =
        DependencyProperty.RegisterAttached(
            "PropertyName",
            typeof(string),
            typeof(GridViewSort),
            new UIPropertyMetadata(null)
        );

    #endregion

    #region Column header click event handler

    private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        GridViewColumnHeader? headerClicked = e.OriginalSource as GridViewColumnHeader;
        if (headerClicked != null)
        {
            if (headerClicked.Column == null)
                return;
            string propertyName = GetPropertyName(headerClicked.Column);
            if (string.IsNullOrEmpty(propertyName)) return;

            var listView = GetAncestor<ListView>(headerClicked);
            if (listView == null) return;
            var command = GetCommand(listView);
            if (command.CanExecute(propertyName))
            {
                command.Execute(propertyName);
            }
            else if (GetAutoSort(listView))
            {
                ApplySort(listView.Items, propertyName);
            }
        }
    }

    #endregion

    #region Helper methods

    public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(reference);
        while (parent is not T)
        {
            if (parent != null) parent = VisualTreeHelper.GetParent(parent);
        }
        return (T)parent;
    }

    public static void ApplySort(ICollectionView view, string propertyName)
    {
        var direction = ListSortDirection.Ascending;
        var currentSort = view.SortDescriptions[0];
        if (view.SortDescriptions.Count > 0)
        {
            if (currentSort.PropertyName == propertyName)
            {
                direction = currentSort.Direction == ListSortDirection.Ascending 
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            view.SortDescriptions.Clear();
        }
        if (!string.IsNullOrEmpty(propertyName))
        {
            view.SortDescriptions.Add(new(propertyName, direction));
        }
    }

    #endregion
}
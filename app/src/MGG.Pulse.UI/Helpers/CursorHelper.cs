using System.Reflection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace MGG.Pulse.UI.Helpers;

public static class CursorHelper
{
    private static readonly InputCursor _handCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    private static readonly PropertyInfo? _protectedCursorProperty = typeof(UIElement).GetProperty(
        "ProtectedCursor",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    public static void ApplyHandCursorToInteractiveElements(FrameworkElement root)
    {
        if (_protectedCursorProperty is null)
        {
            return;
        }

        AttachIfInteractive(root);

        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = VisualTreeHelper.GetChildrenCount(current);

            for (var i = 0; i < children; i++)
            {
                var child = VisualTreeHelper.GetChild(current, i);
                queue.Enqueue(child);

                if (child is FrameworkElement element)
                {
                    AttachIfInteractive(element);
                }
            }
        }
    }

    private static void AttachIfInteractive(FrameworkElement element)
    {
        if (!IsInteractiveElement(element))
        {
            return;
        }

        element.PointerEntered -= Element_PointerEntered;
        element.PointerExited -= Element_PointerExited;
        element.PointerEntered += Element_PointerEntered;
        element.PointerExited += Element_PointerExited;
    }

    private static bool IsInteractiveElement(FrameworkElement element)
    {
        return element is ButtonBase
            || element is NavigationViewItem
            || element is ComboBox
            || element is ToggleSwitch
            || element is RadioButton
            || element is NumberBox;
    }

    private static void Element_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement element)
        {
            _protectedCursorProperty?.SetValue(element, _handCursor);
        }
    }

    private static void Element_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement element)
        {
            _protectedCursorProperty?.SetValue(element, null);
        }
    }
}

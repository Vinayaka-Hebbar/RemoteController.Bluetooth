using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RemoteController.Command
{
    public static class TextEvents
    {
        public static readonly DependencyProperty TextChangedProperty = DependencyProperty.RegisterAttached("TextChanged", typeof(ICommand), typeof(TextEvents), new PropertyMetadata(null, TextChangedAttached));

        public static readonly DependencyProperty KeyDownProperty = DependencyProperty.RegisterAttached("KeyDown", typeof(ICommand), typeof(TextEvents), new PropertyMetadata(null, KeyDownAttached));


        public static ICommand GetTextChanged(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(TextChangedProperty);
            ;
        }

        public static void SetTextChanged(DependencyObject obj, ICommand value)
        {
            obj.SetValue(TextChangedProperty, value);
        }

        public static ICommand GetKeyDown(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(KeyDownProperty);
            ;
        }

        public static void SetKeyDown(DependencyObject obj, ICommand value)
        {
            obj.SetValue(KeyDownProperty, value);
        }

        private static void TextChangedAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb)
            {
                if (e.OldValue is object)
                    tb.TextChanged -= OnTextChanged;
                tb.SetValue(TextChangedProperty, e.NewValue);
                tb.TextChanged += OnTextChanged;
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ICommand command = (ICommand)((DependencyObject)sender).GetValue(TextChangedProperty);
            command.Execute(e);
        }

        private static void KeyDownAttached(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBoxBase tb)
            {
                if (e.OldValue is object)
                    tb.KeyDown -= OnKeyDown;
                tb.SetValue(KeyDownProperty, e.NewValue);
                tb.KeyDown += OnKeyDown;
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            ICommand command = (ICommand)((DependencyObject)sender).GetValue(KeyDownProperty);
            command.Execute(e);
        }
    }
}

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RemoteController.Dialogs
{
    public partial class DialogBase : Window
    {
        public DialogBase()
        {
            CommandBindings.Add(SubmitCommand);
        }

        public static readonly DependencyProperty SubmitLabelProperty = DependencyProperty.Register("SubmitLabel", typeof(string), typeof(DialogBase), new PropertyMetadata("Create"));

        private static readonly CommandBinding SubmitCommand = new CommandBinding(ApplicationCommands.New, OnExecute);

        static DialogBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogBase), new FrameworkPropertyMetadata(typeof(DialogBase)));
        }

        public string SubmitLabel
        {
            get => (string)GetValue(SubmitLabelProperty);
            set => SetValue(SubmitLabelProperty, value);
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Win32.NativeMethods.HideMinimizeAndMaximizeButtons(this);
        }

        private static void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DialogBase dialog && dialog.Validated())
            {
                dialog.OnSubmit();
            }
        }

        protected virtual void OnSubmit()
        {
            DialogResult = true;
        }


        protected virtual bool Validated()
        {
            return true;
        }
    }
}

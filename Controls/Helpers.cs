using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RemoteController.Controls
{
    public static class Helpers
    {
        internal static string GetError(ValidationError error)
        {
            if (error.Exception == null || error.Exception.InnerException == null)
                return error.ErrorContent.ToString();
            else
                return error.Exception.InnerException.Message;
        }

        public static void ShowError(Window window, DependencyObject element)
        {
            StringBuilder sb = new StringBuilder("Following errors has occured\n");
            foreach (var error in Validation.GetErrors(element))
            {
                sb.Append('\t').AppendLine(GetError(error));
            }
            MessageBox.Show(window, sb.ToString(), window.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        public static void ShowError(DependencyObject element)
        {
            ShowError(Window.GetWindow(element), element);
        }
    }
}

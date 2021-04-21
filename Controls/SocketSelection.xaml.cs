using RemoteController.Model;
using System.Net;
using System.Windows.Controls;

namespace RemoteController.Controls
{
    /// <summary>
    /// Interaction logic for SocketSelection.xaml
    /// </summary>
    public partial class SocketSelection
    {
        static readonly ValidationError PasswordRequired = new ValidationError(new Validations.MandatoryRule(), "Text")
        {
            ErrorContent = "Host is required"
        };

        public SocketSelection()
        {
            InitializeComponent();
        }

        public override IDeviceOption GetDeviceOption()
        {
            if (IPAddress.TryParse(HostInput.Text, out var address)
                    && ushort.TryParse(PortInput.Text, out var port))
                return new SocketOption
                {
                    EndPoint = new IPEndPoint(address, port)
                };
            return base.GetDeviceOption();
        }

        public override bool Validate()
        {
            var password = HostInput.GetBindingExpression(TextBox.TextProperty);
            if (password != null)
            {
                password.UpdateSource();
                if (string.IsNullOrEmpty(HostInput.Text))
                {
                    Validation.MarkInvalid(password, PasswordRequired);
                    Helpers.ShowError(HostInput);
                    return false;
                }
            }
            return base.Validate();
        }
    }
}

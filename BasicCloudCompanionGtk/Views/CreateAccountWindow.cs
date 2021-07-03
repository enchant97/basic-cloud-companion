using Gtk;

namespace BasicCloudCompanionGtk.Views
{
    class CreateAccountWindow : Dialog
    {
        private readonly Entry usernameEntry;
        private readonly Entry passwordEntry;
        private readonly Entry passwordConfirmEntry;
        public string Password { get { return passwordEntry.Text; } }
        public string PasswordConfirm { get { return passwordConfirmEntry.Text; } }
        public string Username { get { return usernameEntry.Text; } }
        public CreateAccountWindow(Window parent) : base("Create Account", parent, 0)
        {
            Label captionLabel = new("Create Account");
            ContentArea.PackStart(captionLabel, false, false, 10);

            usernameEntry = new();
            usernameEntry.PlaceholderText = "Enter Username...";
            ContentArea.PackStart(usernameEntry, true, false, 0);

            passwordEntry = new();
            ContentArea.PackStart(passwordEntry, true, false, 0);
            passwordEntry.PlaceholderText = "Enter Password...";

            passwordConfirmEntry = new();
            ContentArea.PackStart(passwordConfirmEntry, true, false, 0);
            passwordConfirmEntry.PlaceholderText = "Confirm Password...";

            AddButton(Stock.Cancel, ResponseType.Cancel);
            AddButton(Stock.Ok, ResponseType.Ok);

            ShowAll();
        }
    }
}

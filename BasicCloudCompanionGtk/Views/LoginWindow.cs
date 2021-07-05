using Gtk;

namespace BasicCloudCompanionGtk.Views
{
    class LoginWindow : Dialog
    {
        private readonly Entry usernameEntry;
        private readonly Entry passwordEntry;
        public LoginWindow(Window parent, string defaultUsername = null) : base("Login", parent, 0)
        {         
            Label captionLabel = new("Please Login");
            ContentArea.PackStart(captionLabel, false, false, 10);

            usernameEntry = new();
            usernameEntry.Text = defaultUsername;
            usernameEntry.PlaceholderText = "Enter Username...";
            ContentArea.PackStart(usernameEntry, true, false, 0);

            passwordEntry = new();
            ContentArea.PackStart(passwordEntry, true, false, 0);
            passwordEntry.PlaceholderText = "Enter Password...";

            AddButton(Stock.Cancel, ResponseType.Cancel);
            AddButton(Stock.Ok, ResponseType.Ok);

            ShowAll();
        }
        public string Password { get { return passwordEntry.Text; } }
        public string Username { get { return usernameEntry.Text; } }
    }
}

using Gtk;
namespace BasicCloudCompanionGtk.Helpers
{
    static class Alerts
    {
        public static void ShowWarning(Window parent, string text)
        {
            MessageDialog dialog = new(
                parent,
                0,
                MessageType.Warning,
                ButtonsType.Ok,
                text
            );
            dialog.Run();
            dialog.Destroy();
        }
        public static void ShowError(Window parent, string text)
        {
            MessageDialog dialog = new(
                parent,
                0,
                MessageType.Error,
                ButtonsType.Ok,
                text
            );
            dialog.Run();
            dialog.Destroy();
        }
    }
}

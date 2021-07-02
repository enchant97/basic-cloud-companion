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
        /// <summary>
        /// Asks the user user a question,
        /// they can either reply with Yes/No
        /// </summary>
        /// <param name="parent">the parent window</param>
        /// <param name="text">the message to show</param>
        /// <returns>what response was given</returns>
        public static ResponseType ShowQuestion(Window parent, string text)
        {
            MessageDialog dialog = new(
                parent,
                0,
                MessageType.Question,
                ButtonsType.YesNo,
                text
            );
            var response = dialog.Run();
            dialog.Destroy();
            return (ResponseType)response;
        }
    }
}

using Gtk;
using System.Net;
using System.Net.Http;

namespace BasicCloudCompanionGtk.Helpers
{
    static class Handlers
    {
        /// <summary>
        /// Handle known HTTP errors and show a alert window
        /// </summary>
        /// <param name="parent">the parent window to add the popup to</param>
        /// <param name="exception">the HTTP exception</param>
        /// <param name="handleUnknown">whether to handle a unknown Http exception</param>
        /// <returns>returns if the exception was handled</returns>
        public static bool ShowHttpExceptionAlert(Window parent, HttpRequestException exception, bool handleUnknown = true)
        {
            if (exception.InnerException is System.Net.Sockets.SocketException)
            {
                Alerts.ShowError(parent, "No server connection");
            }
            else if (exception.StatusCode == HttpStatusCode.Unauthorized)
            {
                Alerts.ShowError(parent, "Not authorised");
            }
            else
            {
                if (handleUnknown)
                {
                    Alerts.ShowError(parent, "Unhandled HTTP error");
                }
                else { return false; }
            }
            return true;
        }
    }
}

using Gtk;

namespace BasicCloudCompanionGtk.Helpers
{
    public static class InputDialogs
    {
        /// <summary>
        /// allows for returning the response type and the content
        /// </summary>
        public record ResponseAndString(ResponseType ResponseType, string Content);
        /// <summary>
        /// Ask the user to select a folder
        /// </summary>
        /// <param name="parent">the parent window</param>
        /// <param name="title">a title for the dialog</param>
        /// <returns>the response and the folder path</returns>
        public static ResponseAndString ShowSelectFolder(Window parent, string title)
        {
            FileChooserDialog dialog = new(
                title,
                parent,
                FileChooserAction.SelectFolder
            );

            dialog.AddButton(Stock.Cancel, ResponseType.Cancel);
            dialog.AddButton(Stock.Save, ResponseType.Ok);

            ResponseType response = (ResponseType)dialog.Run();
            string folderPath = dialog.Filename;
            dialog.Destroy();
            return new ResponseAndString(response, folderPath);
        }
    }
}

using Gtk;

namespace BasicCloudCompanionGtk
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            Views.MainWindow mainWindow = new();
            mainWindow.Show();
            Application.Run();
        }

    }
}

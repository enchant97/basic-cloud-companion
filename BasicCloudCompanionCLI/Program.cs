using System;
using System.Threading;
using static BasicCloudApi.Helpers;

namespace BasicCloudCompanionCLI
{
    class Program
    {
        private static string Username;
        private static string CurrPath;
        private static BasicCloudApi.Communication cloudApi;
        private static BasicCloudApi.Types.DirectoryRoots DirRoots;
        static void Main(string[] args)
        {
            // TODO: get server url from user
            cloudApi = new("http://127.0.0.1:8000");
            ShowStartUp();
            DoLogin();
            ShowMenu();
        }
        static void ShowStartUp()
        {
            Console.WriteLine("Welcome to the Basic-Cloud Companion");
            Thread.Sleep(500);
            Console.WriteLine("CLI edition");
            Thread.Sleep(1500);
            Console.Clear();
        }
        static void ShowMenu()
        {
            bool exitRequested = false;
            while (!exitRequested)
            {
                Console.WriteLine("Menu");
                Console.WriteLine("0, Quit");
                Console.WriteLine("1. Logout");
                Console.WriteLine("2. Navigate");
                Console.Write(">> ");
                bool valid = int.TryParse(Console.ReadLine(), out int choice);
                if (!valid)
                {
                    Console.WriteLine("Invalid Input");
                    Console.ReadKey();
                    Console.Clear();
                    continue;
                }
                switch (choice)
                {
                    case 0:
                        exitRequested = true;
                        break;
                    case 1:
                        Console.Clear();
                        DoLogin();
                        break;
                    case 2:
                        Console.Clear();
                        ShowNavigationMenu();
                        break;
                    default:
                        Console.WriteLine("Invalid Input");
                        Console.ReadKey();
                        Console.Clear();
                        break;
                }
            }
        }
        static string AskLoginDetails()
        {
            Console.WriteLine("Please Login");
            Console.Write("Username: ");
            Username = Console.ReadLine();
            Console.Write("Password: ");
            // TODO: make password hidden on terminal
            string password = Console.ReadLine();
            Console.Clear();
            return password;
        }
        static void DoLogin()
        {
            string password = AskLoginDetails();

            cloudApi.PostLoginToken(Username, password, true).Wait();
            Console.WriteLine("Welcome " + Username);
            Thread.Sleep(500);
            Console.Clear();
        }
        static void ShowNavigationMenu()
        {
            while (true)
            {
                if (CurrPath == null) { DoNavigateToRoot();  }
                else { DoNavigateCurrDir(); }
            }
        }
        static void DoNavigateToRoot()
        {
            Console.WriteLine("Loading root directories...");
            CurrPath = null;
            DirRoots = cloudApi.GetDirectoryRoots().Result;

            bool valid = false;
            while (!valid)
            {
                Console.Clear();
                Console.WriteLine("1. " + DirRoots.shared);
                Console.WriteLine("2. " + DirRoots.home);

                Console.Write(">> ");
                valid = int.TryParse(Console.ReadLine(), out int choice);
                if (!valid)
                {
                    Console.WriteLine("Input Invalid");
                    Console.ReadKey();
                    continue;
                }
                switch (choice)
                {
                    case 1:
                        CurrPath = DirRoots.shared;
                        break;
                    case 2:
                        CurrPath = DirRoots.home;
                        break;
                    default:
                        Console.WriteLine("Input Invalid");
                        Console.ReadKey();
                        valid = false;
                        break;
                }
            }
        }
        static void DoNavigateCurrDir()
        {
            Console.WriteLine("Loading directories...");
            var contents = cloudApi.PostDirectoryContents(CurrPath).Result;
            bool valid = false;
            while (!valid)
            {
                Console.Clear();
                Console.WriteLine("0. ..");
                for (int i = 0; i < contents.Length; i++)
                {
                    Console.WriteLine((i + 1).ToString() + ". " + contents[i].name);
                }
                Console.Write(">> ");
                valid = int.TryParse(Console.ReadLine(), out int choice);
                if (!valid)
                {
                    Console.WriteLine("Input Invalid");
                    Console.ReadKey();
                    continue;
                }
                if (choice < 0 || choice > contents.Length)
                {
                    Console.WriteLine("Input Invalid");
                    Console.ReadKey();
                    valid = false;
                    continue;
                }

                if (choice == 0)
                {
                    if (CurrPath == DirRoots.home || CurrPath == DirRoots.shared)
                    {
                        // Navigate back to root
                        CurrPath = null;
                    }
                    else
                    {
                        // Navigate back a directory
                        CurrPath = GetParentDir(CurrPath);
                    }
                }
                else
                {
                    var current_content = contents[choice - 1];
                    if (current_content.meta.is_directory) { ShowFolderControlMenu(current_content.name); }
                    else { ShowFileControlMenu(current_content.name); }

                }
            }
        }
        static void ShowFileControlMenu(string fileName, bool allowEdit = true)
        {
            bool valid = false;
            while (!valid)
            {
                Console.Clear();
                Console.WriteLine("1. Cancel");
                Console.WriteLine("2. Download");
                if (allowEdit) { Console.WriteLine("3. Delete"); }

                Console.Write(">> ");
                valid = int.TryParse(Console.ReadLine(), out int choice);
                if (!valid)
                {
                    Console.WriteLine("Input Invalid");
                    Console.ReadKey();
                }
                else if (choice == 1) { }
                else if (choice == 2)
                {
                    string downloadPath = ShowFileDialogPicker();
                    if (downloadPath != null)
                    {
                        Console.WriteLine("Downloading File...");
                        var httpContent = cloudApi.DownloadFile(JoinBasePath(CurrPath, fileName)).Result;
                        Console.WriteLine("Writing To System");
                        WriteHttpContentToFile(downloadPath, httpContent).Wait();
                        Console.Clear();
                    }
                }
                else if (choice == 3 && allowEdit == true)
                {
                    string deletePath = JoinBasePath(CurrPath, fileName);
                    Console.Clear();
                    Console.WriteLine("Deleting File...");
                    cloudApi.DeleteFile(deletePath).Wait();
                }
                else
                {
                    Console.WriteLine("Input Invalid");
                    Console.ReadKey();
                    valid = false;
                }
            }
        }
        static void ShowFolderControlMenu(string folderName, bool allowEdit = true)
        {
            bool valid = false;
            while (!valid)
            {
                Console.Clear();
                Console.WriteLine("1. Cancel");
                Console.WriteLine("2. Change Directory");
                Console.WriteLine("3. Download");
                if (allowEdit) { Console.WriteLine("4. Delete"); }

                Console.Write(">> ");
                valid = int.TryParse(Console.ReadLine(), out int choice);
                if (!valid)
                {
                    Console.WriteLine("Input Invalid");
                    Console.ReadKey();
                }
                else if (choice == 1) { }
                else if (choice == 2)
                {

                    CurrPath = JoinBasePath(CurrPath, folderName);
                }
                else if (choice == 3)
                {
                    string downloadPath = ShowFileDialogPicker();
                    if (downloadPath != null)
                    {
                        Console.WriteLine("Downloading Zip...");
                        var httpContent = cloudApi.DownloadDirectoryAsZip(CurrPath + "/" + folderName).Result;
                        Console.WriteLine("Writing To System");
                        WriteHttpContentToFile(downloadPath, httpContent).Wait();
                        Console.Clear();
                    }
                }
                else if (choice == 4 && allowEdit == true)
                {
                    string deletePath = JoinBasePath(CurrPath, folderName);
                    Console.Clear();
                    Console.WriteLine("Deleting Directory...");
                    cloudApi.DeleteDirectory(deletePath).Wait();
                }
                else
                {
                    Console.WriteLine("Input Invalid");
                    Console.ReadKey();
                    valid = false;
                }
            }
        }
        static string ShowFileDialogPicker()
        {
            Console.WriteLine("Enter File Path (or leave blank to exit):");
            string filePath = Console.ReadLine();
            filePath = filePath.Trim();
            Console.Clear();
            if (filePath != string.Empty) { return filePath; }
            return null;
        }
    }
}

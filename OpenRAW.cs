using System;
using System.IO;
using System.Diagnostics;

namespace OpenRAW
{
    class Program
    {
        static void Main(string[] args)
        {
            // check args amount
            if (args.Length != 2) ConsoleInterface.Exits.NotEnoughArgs();

            string editor_path = args[0];
            string file_path = args[1];

            // check paths in args
            if (!File.Exists(editor_path)) ConsoleInterface.Exits.FileNotExists();
            if (!File.Exists(file_path)) ConsoleInterface.Exits.FileNotExists();

            string directory = Path.GetDirectoryName(file_path);
            string file_wildcard = Path.GetFileNameWithoutExtension(file_path) + ".*";

            string[] variants = Directory.GetFiles(directory, file_wildcard);

            Process editor = new Process();
            editor.StartInfo.FileName = editor_path;

            if (variants.Length == 0)
            {
                ConsoleInterface.Exits.FileNotExists();
            }
            else if (variants.Length == 1) // only source file found, open it
            {
                editor.StartInfo.Arguments = file_path;
            }
            else // multiple files, ask user to choose one
            {
                int users_choice = ConsoleInterface.VariantsDialogue(variants);
                editor.StartInfo.Arguments = variants[users_choice];
            }
            editor.Start();
            ConsoleInterface.Exits.Success();
        }
    }

    public static class ConsoleInterface
    {
        // output variants and input desicion
        public static int VariantsDialogue(string[] variants)
        {
            for (int i = 0; i < variants.Length; i++)
            {
                Console.Write((i + 1).ToString() + ". ");

                // mark variant as raw if it's raw
                if (PhotoExtensions.IsExtensionRaw(Path.GetExtension(variants[i])))
                {
                    Console.Write("raw\t");
                }
                else
                {
                    Console.Write("\t");
                }

                Console.WriteLine(Path.GetFileName(variants[i]));
            }
            string q = Console.ReadKey(true).KeyChar.ToString();
            bool parsed = Int32.TryParse(q, out int n);
            if (!parsed) ConsoleInterface.Exits.WrongUserInput();
            n--;
            if (n < 0 || n > variants.Length - 1) ConsoleInterface.Exits.WrongUserInput();
            return n;
        }

        public static class Exits
        {
            public static void Success()
            {
                Console.WriteLine("Opening the file");
                Environment.Exit(0);
            }
            public static void NotEnoughArgs()
            {
                Console.WriteLine("Error: Not enough args given");
                Environment.Exit(1);
            }
            public static void FileNotExists()
            {
                Console.WriteLine("Error: Given path does not exist");
                Environment.Exit(2);
            }
            public static void WrongUserInput()
            {
                Console.WriteLine("Error: Wrong user input");
                Environment.Exit(3);
            }
        }
    }


    public static class PhotoExtensions
    {
        private static string[] raw_extensions = { ".arw", ".dng" };

        public static bool IsExtensionRaw(string new_extension)
        {
			new_extension = new_extension.ToLower();
            foreach (string ext in raw_extensions)
            {
                if (new_extension == ext)
                {
                    return true;
                }
            }
            return false;
        }

    }
}

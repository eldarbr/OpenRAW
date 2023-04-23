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
            if (args.Length != 2 && args.Length != 3) ConsoleInterface.Exits.NotEnoughArgs();

            string editor_path = args[0];
            string file_path = args[1];

            string custom_args = "";
            if (args.Length == 3) custom_args = " " + args[2];

            VariantManager manager = null;
            try
            {
                manager = new VariantManager(editor_path, file_path);
            }
            catch (FileNotFoundException)
            {
                ConsoleInterface.Exits.WrongUserInput();
            }
            catch (FileLoadException)
            {
                ConsoleInterface.Exits.FileNotExists();
            }

            Process editor = new Process();
            editor.StartInfo.FileName = editor_path;


            if (manager.Variants.Length == 1) // only source file found, open it
            {
                editor.StartInfo.Arguments = manager.Variants[0].Filepath;
            }
            else // multiple files, ask user to choose one
            {
                int users_choice = ConsoleInterface.VariantsDialogue(manager.Variants);
                editor.StartInfo.Arguments = manager.Variants[users_choice].Filepath + custom_args;
            }
            editor.Start();
            ConsoleInterface.Exits.Success();
        }
    }

    public class VariantManager
    {
        private string directory;
        private string file_wildcard;
        private FileVariants[] variants;

        public VariantManager(string editor_path, string file_path)
        {
            // check paths in args
            if (!File.Exists(editor_path)) throw new FileNotFoundException();
            if (!File.Exists(file_path)) throw new FileNotFoundException();

            directory = Path.GetDirectoryName(file_path);
            file_wildcard = Path.GetFileNameWithoutExtension(file_path) + ".*";

            SearchVariants();
        }

        // finds file variants and sorts the array
        public void SearchVariants()
        {
            string[] foundFiles = Directory.GetFiles(directory, file_wildcard);

            if (foundFiles.Length == 0)
            {
                throw new FileLoadException();
            }

            variants = new FileVariants[foundFiles.Length];
            for (int i = 0; i < foundFiles.Length; i++)
            {
                variants[i] = new FileVariants(foundFiles[i]);
            }
            SortVariants();
        }

        // puts all raw files at the beginning of array
        private void SortVariants()
        {
            int last_raw = -1;
            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i].IsRaw)
                {
                    FileVariants tmp = variants[last_raw + 1];
                    variants[last_raw + 1] = variants[i];
                    variants[i] = tmp;
                    last_raw = i;
                }
            }
        }

        public FileVariants[] Variants { get { return variants; } }
    }

    public static class ConsoleInterface
    {
        // output variants and input desicion
        public static int VariantsDialogue(FileVariants[] variants)
        {
            for (int i = 0; i < variants.Length; i++)
            {
                Console.Write((i + 1).ToString() + ". ");

                // mark variant as raw if it's raw
                if (variants[i].IsRaw)
                {
                    Console.Write("raw\t");
                }
                else
                {
                    Console.Write("\t");
                }

                Console.WriteLine(variants[i].Filename);
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

    public class FileVariants
    {
        private string filepath;
        private string filename;
        private string extension;
        private bool isRaw;

        public FileVariants(string path)
        {
            filepath = path;
            filename = Path.GetFileName(path);
            extension = Path.GetExtension(path);
            isRaw = PhotoExtensions.IsExtensionRaw(extension);
        }

        public bool IsRaw { get { return isRaw; } }
        public string Filename { get { return filename; } }
        public string Filepath { get { return filepath; } }
    }
}

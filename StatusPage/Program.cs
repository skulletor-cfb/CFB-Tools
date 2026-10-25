using System.Diagnostics;
using MaddenFranchise;

namespace StatusPage
{
    /// <summary>
    /// Builds a status page for a CFB 27 dynasty save: every game, by conference, with the latest simulated week's scores
    /// hidden behind a button. Reads the newest DYNASTY-* save (or the save / folder given on the command line), writes
    /// status.html to the current directory and opens it. The save is only read.
    /// </summary>
    internal static class Program
    {
        private const string OutputFile = "status.html";

        private static int Main(string[] args)
        {
            var open = !args.Contains("--no-open");
            var target = args.FirstOrDefault(a => !a.StartsWith("--"));

            var save = FindSave(target);
            if (save == null)
            {
                Console.Error.WriteLine($"No DYNASTY-* save found in {target ?? AppContext.BaseDirectory + " or " + DefaultSavesFolder}.");
                return 1;
            }

            Console.WriteLine($"Reading {save}");
            var page = SeasonReader.Read(FranchiseFile.Load(save), Path.GetFileName(save));
            var path = Path.Combine(Environment.CurrentDirectory, OutputFile);
            File.WriteAllText(path, PageWriter.Write(page));
            Console.WriteLine($"{page.Games.Count} games, latest simulated week: {page.Weeks.FirstOrDefault(w => w.Week == page.LatestWeek)?.Label ?? "none"}. Wrote {path}");

            if (open)
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }

            return 0;
        }

        private static string DefaultSavesFolder =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EA SPORTS College Football 27", "saves");

        /// <summary>
        /// The save file given, or the most recently written DYNASTY-* save in the folder given. With neither, the folder the
        /// program sits in (published into the saves folder, as on a Mac reading the synced saves), then the game's saves folder.
        /// </summary>
        private static string? FindSave(string? target)
        {
            if (target != null && File.Exists(target))
            {
                return target;
            }

            var folders = target != null ? new[] { target } : new[] { AppContext.BaseDirectory, DefaultSavesFolder };
            return folders.Select(NewestSave).FirstOrDefault(save => save != null);
        }

        private static string? NewestSave(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return null;
            }

            return new DirectoryInfo(folder).GetFiles("DYNASTY-*")
                .Where(f => f.Extension.Length == 0)
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault()?.FullName;
        }
    }
}

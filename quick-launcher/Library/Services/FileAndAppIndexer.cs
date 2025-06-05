using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary; // COM reference required

namespace quick_launcher.Library.Services
{
    public class FileAndAppIndexer
    {
        public IEnumerable<string> SearchFiles(string rootPath, string pattern = "*.*")
        {
            try
            {
                return Directory.EnumerateFiles(rootPath, pattern, SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                // Log or handle exceptions (e.g., access denied)
                return Enumerable.Empty<string>();
            }
        }
        public IEnumerable<string> GetRecentItems()
        {
            string recentPath = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            return Directory.EnumerateFiles(recentPath, "*.lnk", SearchOption.TopDirectoryOnly);
        }
        public IEnumerable<string> GetStartMenuShortcuts()
        {
            string userPrograms = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string commonPrograms = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);

            var shortcuts = Directory.EnumerateFiles(userPrograms, "*.lnk", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(commonPrograms, "*.lnk", SearchOption.AllDirectories));

            return shortcuts;
        }
        public string ResolveShortcut(string shortcutPath)
        {
            try
            {
                var shell = new WshShell();
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                return link.TargetPath;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<(string ShortcutName, string TargetPath)> GetResolvedStartMenuApps()
        {
            foreach (var lnk in GetStartMenuShortcuts())
            {
                string target = ResolveShortcut(lnk);
                if (!string.IsNullOrEmpty(target))
                {
                    yield return (Path.GetFileNameWithoutExtension(lnk), target);
                }
            }
        }
    }
}

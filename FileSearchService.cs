using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CustomSearchSpotlight
{
    public class FileSearchResult
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime Modified { get; set; }
        public long Size { get; set; }
        public string Icon { get; set; }
        
        public string Description => $"{Type} • {GetSizeString()} • {Modified:dd.MM.yyyy}";
        
        private string GetSizeString()
        {
            if (Size < 1024) return $"{Size} B";
            if (Size < 1024 * 1024) return $"{Size / 1024} KB";
            return $"{Size / (1024 * 1024)} MB";
        }
    }

    public class FileSearchService
    {
        private readonly List<string> _searchPaths = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
        };

        private readonly Dictionary<string, string> _fileTypeIcons = new Dictionary<string, string>
        {
            { ".txt", "📄" }, { ".doc", "📝" }, { ".docx", "📝" },
            { ".pdf", "📕" }, { ".xls", "📊" }, { ".xlsx", "📊" },
            { ".jpg", "🖼️" }, { ".png", "🖼️" }, { ".gif", "🖼️" },
            { ".mp3", "🎵" }, { ".mp4", "🎬" }, { ".avi", "🎬" },
            { ".zip", "📦" }, { ".rar", "📦" }, { ".exe", "⚙️" },
            { ".lnk", "🔗" }
        };

        public async Task<List<FileSearchResult>> SearchAsync(string query, int maxResults = 20)
        {
            return await Task.Run(() => SearchFiles(query, maxResults));
        }

        private List<FileSearchResult> SearchFiles(string query, int maxResults)
        {
            var results = new List<FileSearchResult>();
            var queryLower = query.ToLower();

            foreach (var searchPath in _searchPaths)
            {
                if (!Directory.Exists(searchPath)) continue;

                try
                {
                    SearchInDirectory(searchPath, queryLower, results, maxResults);
                    if (results.Count >= maxResults) break;
                }
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
                catch (Exception) { }
            }

            return results.Take(maxResults).ToList();
        }

        private void SearchInDirectory(string directory, string query, List<FileSearchResult> results, int maxResults)
        {
            try
            {
                // Search files
                foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
                {
                    if (results.Count >= maxResults) return;

                    var fileName = Path.GetFileName(file);
                    if (fileName.ToLower().Contains(query))
                    {
                        var fileInfo = new FileInfo(file);
                        var extension = Path.GetExtension(file).ToLower();

                        results.Add(new FileSearchResult
                        {
                            Name = fileName,
                            Path = file,
                            Type = GetFileType(extension),
                            Modified = fileInfo.LastWriteTime,
                            Size = fileInfo.Length,
                            Icon = GetFileIcon(extension)
                        });
                    }
                }
            }
            catch { }
        }

        private string GetFileType(string extension)
        {
            return extension switch
            {
                ".txt" => "Text File",
                ".doc" or ".docx" => "Word Document",
                ".pdf" => "PDF Document",
                ".xls" or ".xlsx" => "Excel Spreadsheet",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "Image",
                ".mp3" or ".wav" or ".flac" => "Audio",
                ".mp4" or ".avi" or ".mkv" or ".mov" => "Video",
                ".zip" or ".rar" or ".7z" => "Archive",
                ".exe" or ".msi" => "Application",
                ".lnk" => "Shortcut",
                _ => "File"
            };
        }

        private string GetFileIcon(string extension)
        {
            return _fileTypeIcons.ContainsKey(extension) ? _fileTypeIcons[extension] : "📄";
        }

        public List<string> GetCommonSearchLocations()
        {
            return new List<string>
            {
                "Desktop",
                "Documents", 
                "Downloads",
                "Pictures",
                "Music",
                "Videos"
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CustomSearchApp
{
    public class FileSearchResult
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime Modified { get; set; }
        public long Size { get; set; }
        public ImageSource Icon { get; set; }
        
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
        private readonly List<string> _searchPaths;
        private readonly HashSet<string> _allowedExtensions;
        
        public FileSearchService()
        {
            // Common search locations
            _searchPaths = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };
            
            // Common file extensions
            _allowedExtensions = new HashSet<string>
            {
                ".txt", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".mp3", ".mp4", ".avi",
                ".zip", ".rar", ".exe", ".lnk", ".url"
            };
        }
        
        public async Task<List<FileSearchResult>> SearchFilesAsync(string query, int maxResults = 20)
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
                    // Search in current directory
                    SearchDirectory(searchPath, queryLower, results, maxResults);
                    
                    // Search in subdirectories (limited depth)
                    if (results.Count < maxResults)
                    {
                        SearchSubdirectories(searchPath, queryLower, results, maxResults, 2);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Search error in {searchPath}: {ex.Message}");
                }
                
                if (results.Count >= maxResults) break;
            }
            
            // Sort by relevance
            return results
                .OrderByDescending(f => GetRelevanceScore(f, queryLower))
                .ThenByDescending(f => f.Modified)
                .Take(maxResults)
                .ToList();
        }
        
        private void SearchDirectory(string directory, string query, 
            List<FileSearchResult> results, int maxResults)
        {
            try
            {
                // Search files
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (results.Count >= maxResults) return;
                    
                    var fileName = Path.GetFileName(file);
                    if (fileName.ToLower().Contains(query))
                    {
                        results.Add(CreateFileResult(file));
                    }
                }
            }
            catch { /* Ignore access errors */ }
        }
        
        private void SearchSubdirectories(string directory, string query, 
            List<FileSearchResult> results, int maxResults, int maxDepth, int currentDepth = 0)
        {
            if (currentDepth >= maxDepth) return;
            
            try
            {
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    if (results.Count >= maxResults) return;
                    
                    SearchDirectory(subDir, query, results, maxResults);
                    
                    if (results.Count < maxResults)
                    {
                        SearchSubdirectories(subDir, query, results, maxResults, 
                            maxDepth, currentDepth + 1);
                    }
                }
            }
            catch { /* Ignore access errors */ }
        }
        
        private FileSearchResult CreateFileResult(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var extension = Path.GetExtension(filePath).ToLower();
            
            return new FileSearchResult
            {
                Name = Path.GetFileName(filePath),
                Path = filePath,
                Type = GetFileType(extension),
                Modified = fileInfo.LastWriteTime,
                Size = fileInfo.Length,
                Icon = GetFileIcon(extension)
            };
        }
        
        private string GetFileType(string extension)
        {
            return extension switch
            {
                ".txt" => "Text Document",
                ".pdf" => "PDF Document",
                ".doc" or ".docx" => "Word Document",
                ".xls" or ".xlsx" => "Excel Document",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "Image",
                ".mp3" or ".wav" or ".flac" => "Audio",
                ".mp4" or ".avi" or ".mkv" => "Video",
                ".exe" => "Application",
                ".lnk" => "Shortcut",
                ".zip" or ".rar" or ".7z" => "Archive",
                _ => "File"
            };
        }
        
        private ImageSource GetFileIcon(string extension)
        {
            // Simple icon based on file type
            var iconPath = extension switch
            {
                ".txt" => "📄",
                ".pdf" => "📕",
                ".doc" or ".docx" => "📘",
                ".xls" or ".xlsx" => "📗",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "🖼️",
                ".mp3" or ".wav" => "🎵",
                ".mp4" or ".avi" => "🎬",
                ".exe" => "⚙️",
                ".lnk" => "🔗",
                ".zip" or ".rar" => "📦",
                _ => "📄"
            };
            
            // In real app, you'd use System.Drawing.Icon or WPF's Icon
            return null; // Return null for text icon for now
        }
        
        private int GetRelevanceScore(FileSearchResult file, string query)
        {
            var score = 0;
            var nameLower = file.Name.ToLower();
            
            // Exact match gets highest score
            if (nameLower == query) score += 100;
            
            // Starts with query
            if (nameLower.StartsWith(query)) score += 50;
            
            // Contains query
            if (nameLower.Contains(query)) score += 25;
            
            // Recent files get bonus
            var daysOld = (DateTime.Now - file.Modified).TotalDays;
            if (daysOld < 7) score += 20;
            else if (daysOld < 30) score += 10;
            
            return score;
        }
        
        public List<string> GetQuickAccessLocations()
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

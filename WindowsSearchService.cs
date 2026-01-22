using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace CustomSearchApp
{
    public class WindowsSearchService
    {
        private const string ConnectionString = @"Provider=Search.CollatorDSO;Extended Properties='Application=Windows'";

        public async Task<List<FileSearchResult>> SearchFilesAsync(string query, int maxResults = 20)
        {
            var results = new List<FileSearchResult>();

            if (string.IsNullOrWhiteSpace(query))
                return results;

            try
            {
                using (var connection = new OleDbConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    
                    // Zapytanie do Windows Search
                    string sqlQuery = @"
                        SELECT 
                            System.ItemName, 
                            System.ItemPathDisplay, 
                            System.ItemType,
                            System.ItemTypeText,
                            System.Size,
                            System.DateModified,
                            System.Kind
                        FROM SYSTEMINDEX
                        WHERE CONTAINS(System.ItemName, '""*" + query + "*""') 
                           OR CONTAINS(System.ItemPathDisplay, '""*" + query + "*""')
                        ORDER BY System.DateModified DESC";

                    using (var command = new OleDbCommand(sqlQuery, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int count = 0;
                            while (await reader.ReadAsync() && count < maxResults)
                            {
                                var result = new FileSearchResult
                                {
                                    Name = reader["System.ItemName"]?.ToString() ?? "Unknown",
                                    Path = reader["System.ItemPathDisplay"]?.ToString() ?? "",
                                    Type = reader["System.ItemTypeText"]?.ToString() ?? "File",
                                    Size = Convert.ToInt64(reader["System.Size"] ?? 0),
                                    Modified = Convert.ToDateTime(reader["System.DateModified"] ?? DateTime.MinValue),
                                    Kind = reader["System.Kind"]?.ToString() ?? ""
                                };
                                
                                results.Add(result);
                                count++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error or fallback to basic file search
                Console.WriteLine($"Windows Search error: {ex.Message}");
                results = await FallbackSearchAsync(query, maxResults);
            }

            return results;
        }

        private async Task<List<FileSearchResult>> FallbackSearchAsync(string query, int maxResults)
        {
            // Fallback to the FileSearchService we created earlier
            var fallbackService = new FileSearchService();
            return await fallbackService.SearchFilesAsync(query, maxResults);
        }
    }

    public class FileSearchResult
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Kind { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        
        public string Description => $"{Type} • {GetSizeString()} • {Modified:dd.MM.yyyy}";
        
        private string GetSizeString()
        {
            if (Size < 1024) return $"{Size} B";
            if (Size < 1024 * 1024) return $"{Size / 1024} KB";
            return $"{Size / (1024 * 1024)} MB";
        }
    }
}
// W WindowsSearchService.cs dodaj metodę:
public async Task<List<FileSearchResult>> SearchByKindAsync(string query, string kind, int maxResults = 20)
{
    var results = new List<FileSearchResult>();
    
    try
    {
        using (var connection = new OleDbConnection(ConnectionString))
        {
            await connection.OpenAsync();
            
            string sqlQuery = @"
                SELECT 
                    System.ItemName, 
                    System.ItemPathDisplay, 
                    System.ItemTypeText,
                    System.Size,
                    System.DateModified,
                    System.Kind
                FROM SYSTEMINDEX
                WHERE (CONTAINS(System.ItemName, '""*" + query + "*""') 
                    OR CONTAINS(System.ItemPathDisplay, '""*" + query + "*""'))
                    AND System.Kind = @kind
                ORDER BY System.DateModified DESC";

            using (var command = new OleDbCommand(sqlQuery, connection))
            {
                command.Parameters.AddWithValue("@kind", kind);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    int count = 0;
                    while (await reader.ReadAsync() && count < maxResults)
                    {
                        var result = new FileSearchResult
                        {
                            Name = reader["System.ItemName"]?.ToString() ?? "Unknown",
                            Path = reader["System.ItemPathDisplay"]?.ToString() ?? "",
                            Type = reader["System.ItemTypeText"]?.ToString() ?? "File",
                            Size = Convert.ToInt64(reader["System.Size"] ?? 0),
                            Modified = Convert.ToDateTime(reader["System.DateModified"] ?? DateTime.MinValue),
                            Kind = reader["System.Kind"]?.ToString() ?? ""
                        };
                        
                        results.Add(result);
                        count++;
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Windows Search by kind error: {ex.Message}");
    }
    
    return results;
}
// Przykładowa, uproszczona implementacja wyszukiwania plików
public class WindowsSearchService
{
    public IEnumerable<SearchResult> SearchFiles(string query, string filter = "*.*")
    {
        var results = new List<SearchResult>();
        // Wyszukiwanie w folderze użytkownika i dyskach systemowych
        string[] searchPaths = {
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            @"C:\Program Files",
            @"C:\Program Files (x86)"
        };
        foreach (var path in searchPaths)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    var files = Directory.EnumerateFiles(path, filter,
                        SearchOption.AllDirectories)
                        .Where(f => f.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .Take(20); // Limit wyników
                    foreach (var file in files)
                    {
                        results.Add(new SearchResult
                        {
                            Title = Path.GetFileName(file),
                            Path = file,
                            ResultType = SearchResultType.File
                        });
                    }
                }
                catch { } // Pomijamy niedostępne ścieżki
            }
        }
        return results;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CustomSearchApp.Services
{
    public class WindowsSearchService
    {
        public enum SearchResultType
        {
            File,
            Application,
            Folder
        }

        public class SearchResult
        {
            public string Title { get; set; }
            public string Path { get; set; }
            public SearchResultType Type { get; set; }
            public string Icon { get; set; }
        }

        public async Task<List<SearchResult>> SearchAsync(string query, int maxResults = 20)
        {
            return await Task.Run(() =>
            {
                var results = new List<SearchResult>();
                
                if (string.IsNullOrWhiteSpace(query))
                    return results;

                query = query.ToLower();
                
                // Wyszukiwanie aplikacji w menu Start
                SearchApplications(query, results, maxResults);
                
                // Wyszukiwanie plików w głównych folderach użytkownika
                SearchUserFiles(query, results, maxResults);
                
                return results.Take(maxResults).ToList();
            });
        }

        private void SearchApplications(string query, List<SearchResult> results, int maxResults)
        {
            try
            {
                string startMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    "Programs");
                
                string commonStartMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                    "Programs");

                SearchInDirectory(startMenuPath, query, results, SearchResultType.Application, "📱");
                SearchInDirectory(commonStartMenuPath, query, results, SearchResultType.Application, "📱");
            }
            catch { /* Ignoruj błędy dostępu */ }
        }

        private void SearchUserFiles(string query, List<SearchResult> results, int maxResults)
        {
            try
            {
                string[] searchDirectories = {
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                foreach (var directory in searchDirectories)
                {
                    if (Directory.Exists(directory))
                    {
                        SearchInDirectory(directory, query, results, SearchResultType.File, "📄");
                        
                        if (results.Count >= maxResults)
                            break;
                    }
                }
            }
            catch { /* Ignoruj błędy dostępu */ }
        }

        private void SearchInDirectory(string directory, string query, List<SearchResult> results, 
                                     SearchResultType type, string icon)
        {
            try
            {
                // Wyszukiwanie plików
                var files = Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly)
                    .Where(f => Path.GetFileName(f).ToLower().Contains(query))
                    .Take(10);

                foreach (var file in files)
                {
                    results.Add(new SearchResult
                    {
                        Title = Path.GetFileName(file),
                        Path = file,
                        Type = type,
                        Icon = icon
                    });
                }

                // Wyszukiwanie folderów
                var directories = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly)
                    .Where(d => Path.GetFileName(d).ToLower().Contains(query))
                    .Take(5);

                foreach (var dir in directories)
                {
                    results.Add(new SearchResult
                    {
                        Title = Path.GetFileName(dir),
                        Path = dir,
                        Type = SearchResultType.Folder,
                        Icon = "📁"
                    });
                }
            }
            catch { /* Ignoruj błędy dostępu do pojedynczego folderu */ }
        }
    }
}

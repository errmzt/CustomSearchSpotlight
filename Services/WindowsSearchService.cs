using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using CustomSearchSpotlight.Models;

namespace CustomSearchSpotlight.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<SearchResult>> SearchAsync(string query);
    }

    public class WindowsSearchService : ISearchService
    {
        public async Task<IEnumerable<SearchResult>> SearchAsync(string query)
        {
            var results = new List<SearchResult>();

            // Using Windows Search API via OleDb
            string connectionString = @"Provider=Search.CollatorDSO;Extended Properties='Application=Windows'";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();

                // Search for files and folders
                string sql = @"
                    SELECT System.ItemName, System.ItemPathDisplay, System.ItemType
                    FROM SYSTEMINDEX
                    WHERE System.ItemName LIKE '%" + query + @"%' 
                    OR System.ItemPathDisplay LIKE '%" + query + @"%'
                    ORDER BY System.Search.Rank DESC";

                using (OleDbCommand command = new OleDbCommand(sql, connection))
                {
                    using (OleDbDataReader reader = (OleDbDataReader)await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string name = reader["System.ItemName"].ToString();
                            string path = reader["System.ItemPathDisplay"].ToString();
                            string type = reader["System.ItemType"].ToString();

                            results.Add(new SearchResult
                            {
                                Title = name,
                                Description = path,
                                Path = path,
                                Type = type.Contains("Folder") ? ResultType.Folder : ResultType.File
                            });
                        }
                    }
                }
            }

            return results;
        }
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
        // Główna metoda wyszukująca
        public async Task<List<SearchResult>> SearchAsync(string query, int maxResults = 15)
        {
            return await Task.Run(() =>
            {
                var results = new List<SearchResult>();

                if (string.IsNullOrWhiteSpace(query))
                    return results;

                string lowerQuery = query.ToLower();

                // 1. Wyszukaj aplikacje w Menu Start
                SearchApplications(lowerQuery, results);

                // 2. Wyszukaj pliki w folderze użytkownika
                SearchUserFiles(lowerQuery, results, maxResults);

                return results.Take(maxResults).ToList();
            });
        }

        private void SearchApplications(string query, List<SearchResult> results)
        {
            try
            {
                string startMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    "Programs");

                string commonStartMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                    "Programs");

                // Przeszukaj skróty .lnk
                SearchInDirectory(startMenuPath, "*.lnk", query, results, "📱");
                SearchInDirectory(commonStartMenuPath, "*.lnk", query, results, "📱");
            }
            catch { /* Ignoruj błędy dostępu */ }
        }

        private void SearchUserFiles(string query, List<SearchResult> results, int maxResults)
        {
            string[] searchPaths = {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            foreach (var path in searchPaths)
            {
                if (Directory.Exists(path))
                {
                    SearchInDirectory(path, "*.*", query, results, "📄");
                    if (results.Count >= maxResults) break;
                }
            }
        }

        private void SearchInDirectory(string path, string searchPattern, string query, 
                                     List<SearchResult> results, string icon)
        {
            try
            {
                // Wyszukaj pliki
                var files = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories)
                    .Where(f => Path.GetFileName(f).ToLower().Contains(query))
                    .Take(10);

                foreach (var file in files)
                {
                    results.Add(new SearchResult
                    {
                        Title = Path.GetFileName(file),
                        Path = file,
                        Icon = icon,
                        Type = Path.GetExtension(file).ToLower() == ".lnk" 
                               ? SearchResultType.Application 
                               : SearchResultType.File
                    });
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (PathTooLongException) { }
        }
    }

    // Model wyniku wyszukiwania
    public class SearchResult
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public SearchResultType Type { get; set; }
    }

    public enum SearchResultType
    {
        File,
        Application,
        Folder,
        Web
    }
}

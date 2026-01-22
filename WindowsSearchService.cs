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

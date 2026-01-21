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

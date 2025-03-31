using Dapper;
using InvestmentPortfolio.Model.Models;
using InvestmentPortfolio.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolio.Repository.Repository
{
    public class VaultRepository:IVaultRepository
    {
        private readonly IConfiguration _configuration;

        public VaultRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadExcelFileAsync(IFormFile file, string fileName)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            var fileType = file.ContentType; // Get MIME type

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            await connection.ExecuteAsync(
            "sp_InsertFile",
                new { FileName = file.FileName, FileContent = fileData, FileType = fileType, FileTypeId = 1},
                commandType: CommandType.StoredProcedure
            );

            return "File uploaded successfully.";
        }

        public async Task<List<dynamic>> GetAllFilesAsync()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var files = await connection.QueryAsync("sp_GetAllFiles", commandType: CommandType.StoredProcedure);
            return files.ToList();
        }

        public async Task<FileModel> DownloadFileAsync(int fileId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var fileData = await connection.QueryFirstOrDefaultAsync<FileModel>(
                "sp_GetFileByIdForDownload",
                new { FileId = fileId },
                commandType: CommandType.StoredProcedure
            );

            return fileData;
        }


        public async Task<string> DeleteFileAsync(int fileId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            int rowsAffected = await connection.ExecuteAsync(
                "sp_DeleteFile",
                new { Id = fileId },
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected > 0 ? "File deleted successfully." : "File not found.";
        }

        /*        public async Task<(byte[] FileContent, string FileType)> ViewFileAsync(int fileId)
                {
                    using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                    await connection.OpenAsync();

                    var fileData = await connection.QueryFirstOrDefaultAsync<(byte[], string)>(
                        "sp_GetFileForView",
                        new { FileId = fileId },
                        commandType: CommandType.StoredProcedure
                    );

                    return fileData;
                }*/

        public async Task<(byte[] FileContent, string FileType)> ViewFileAsync(int fileId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var fileData = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetFileForView",
                new { FileId = fileId },
                commandType: CommandType.StoredProcedure
            );

            if(fileData == null)
                return (Array.Empty<byte>(), "application/octet-stream");

            // ✅ Correct Mapping
            byte[] fileContent = (byte[])fileData.FileContent;
            string fileType = fileData.FileType.ToString();

            return (fileContent, fileType);
        }


    }
}

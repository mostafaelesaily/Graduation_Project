using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.infrastructure.Services
{
    public class FileService : IFileService
    {
        public FileService(ILogger<FileService> logger)
        {
            this.logger = logger;
        }
        private readonly ILogger<FileService> logger;
        public async Task DeleteAsync(string filePath)
        {
            if (filePath == null)
            {
                throw new  ArgumentNullException(nameof(filePath));
            }
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                logger.LogInformation($"File deleted: {fullPath}");
            }
            else
            {
                logger.LogWarning($"File not found: {fullPath}");
            }
        }

        public async Task<string> UpdateFileAsync(IFormFile _file, string folderName, string oldFilePath)
        {
            if (_file == null) throw new ArgumentNullException(nameof(_file));
            if (string.IsNullOrEmpty(oldFilePath)) throw new ArgumentNullException(nameof(oldFilePath));
            if (string.IsNullOrEmpty(folderName)) throw new ArgumentNullException(nameof(folderName));
            var newFilePath = await UploadFileAsync(_file, folderName);
            var fullOldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldFilePath);
            if (File.Exists(fullOldFilePath))
            {
                File.Delete(fullOldFilePath);
                logger.LogInformation($"Old file deleted: {fullOldFilePath}");
            }
            else
            {
                logger.LogWarning($"Old file not found: {fullOldFilePath}");
            }
            return newFilePath;
        }

        public async Task<string> UploadFileAsync(IFormFile File, string folderName)
        {
            logger.LogInformation($"Uploading file: {File.FileName} to folder: {folderName}");
            if (File == null) throw new ArgumentNullException(nameof(File));
            if (string.IsNullOrEmpty(folderName)) throw new ArgumentNullException(nameof(folderName));
            string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileExtension = Path.GetExtension(File.FileName);
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(FolderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                File.CopyTo(stream);
            }
            logger.LogInformation($"File uploaded successfully: {filePath}");
            return Path.Combine(folderName, fileName);
        }
    }
}

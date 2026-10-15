using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile File , string folderName);
        Task<string> UpdateFileAsync(IFormFile File, string folderName, string oldFilePath);
        Task DeleteAsync(string filePath);
    }
}

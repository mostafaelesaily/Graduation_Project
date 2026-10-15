using System.Text;
using MedLink.infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedLink.Test.Services
{
    public class FileServiceTest : IDisposable
    {
        private readonly Mock<ILogger> loggerMock = new();
        private readonly FileService sut;
        private readonly string testFolder = "test-uploads-" + Guid.NewGuid();
        private readonly string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        public FileServiceTest()
        {
            sut = new FileService(loggerMock.Object);
        }

        private static Mock<IFormFile> CreateFormFileMock(string fileName, string content = "file content")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(bytes.Length);
            fileMock.Setup(f => f.CopyTo(It.IsAny<Stream>()))
                .Callback<Stream>(stream => stream.Write(bytes, 0, bytes.Length));
            return fileMock;
        }

        public void Dispose()
        {
            var folderPath = Path.Combine(wwwrootPath, testFolder);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, recursive: true);
            }
        }

        // ---------- UploadFileAsync ----------

        [Fact]
        public async Task UploadFileAsync_NullFile_ThrowsNullReferenceException()
        {
          
            await Assert.ThrowsAsync<NullReferenceException>(() => sut.UploadFileAsync(null!, testFolder));
        }

        [Fact]
        public async Task UploadFileAsync_EmptyFolderName_ThrowsArgumentNullException()
        {
            var fileMock = CreateFormFileMock("test.txt");

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UploadFileAsync(fileMock.Object, string.Empty));
        }

        [Fact]
        public async Task UploadFileAsync_ValidFile_CreatesFolderAndSavesFileWithGuidName()
        {
            var fileMock = CreateFormFileMock("photo.png", "image-bytes");

            var relativePath = await sut.UploadFileAsync(fileMock.Object, testFolder);

            Assert.StartsWith(testFolder, relativePath);
            Assert.EndsWith(".png", relativePath);

            var fullPath = Path.Combine(wwwrootPath, relativePath);
            Assert.True(File.Exists(fullPath));
            Assert.Equal("image-bytes", File.ReadAllText(fullPath));
        }

        [Fact]
        public async Task UploadFileAsync_FolderDoesNotExist_CreatesIt()
        {
            var folderPath = Path.Combine(wwwrootPath, testFolder);
            Assert.False(Directory.Exists(folderPath));
            var fileMock = CreateFormFileMock("doc.pdf");

            await sut.UploadFileAsync(fileMock.Object, testFolder);

            Assert.True(Directory.Exists(folderPath));
        }

        // ---------- UpdateFileAsync ----------

        [Fact]
        public async Task UpdateFileAsync_NullFile_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.UpdateFileAsync(null!, testFolder, "old/path.txt"));
        }

        [Fact]
        public async Task UpdateFileAsync_EmptyOldFilePath_ThrowsArgumentNullException()
        {
            var fileMock = CreateFormFileMock("new.txt");

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.UpdateFileAsync(fileMock.Object, testFolder, string.Empty));
        }

        [Fact]
        public async Task UpdateFileAsync_EmptyFolderName_ThrowsArgumentNullException()
        {
            var fileMock = CreateFormFileMock("new.txt");

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.UpdateFileAsync(fileMock.Object, string.Empty, "old/path.txt"));
        }

        [Fact]
        public async Task UpdateFileAsync_ExistingOldFile_DeletesOldFileAndUploadsNew()
        {
            var oldFileMock = CreateFormFileMock("old.txt", "old content");
            var oldRelativePath = await sut.UploadFileAsync(oldFileMock.Object, testFolder);
            var oldFullPath = Path.Combine(wwwrootPath, oldRelativePath);
            Assert.True(File.Exists(oldFullPath));

            var newFileMock = CreateFormFileMock("new.txt", "new content");
            var newRelativePath = await sut.UpdateFileAsync(newFileMock.Object, testFolder, oldRelativePath);

            Assert.False(File.Exists(oldFullPath));
            var newFullPath = Path.Combine(wwwrootPath, newRelativePath);
            Assert.True(File.Exists(newFullPath));
            Assert.Equal("new content", File.ReadAllText(newFullPath));
        }

        [Fact]
        public async Task UpdateFileAsync_OldFileMissing_StillUploadsNewFile()
        {
            var newFileMock = CreateFormFileMock("new.txt", "new content");

            var newRelativePath = await sut.UpdateFileAsync(
                newFileMock.Object, testFolder, Path.Combine(testFolder, "does-not-exist.txt"));

            var newFullPath = Path.Combine(wwwrootPath, newRelativePath);
            Assert.True(File.Exists(newFullPath));
        }

        // ---------- DeleteAsync ----------

        [Fact]
        public async Task DeleteAsync_NullPath_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.DeleteAsync(null!));
        }

        [Fact]
        public async Task DeleteAsync_ExistingFile_RemovesFile()
        {
            var fileMock = CreateFormFileMock("to-delete.txt");
            var relativePath = await sut.UploadFileAsync(fileMock.Object, testFolder);
            var fullPath = Path.Combine(wwwrootPath, relativePath);
            Assert.True(File.Exists(fullPath));

            await sut.DeleteAsync(relativePath);

            Assert.False(File.Exists(fullPath));
        }

        [Fact]
        public async Task DeleteAsync_FileDoesNotExist_DoesNotThrow()
        {
            var missingPath = Path.Combine(testFolder, "missing.txt");

            var exception = await Record.ExceptionAsync(() => sut.DeleteAsync(missingPath));

            Assert.Null(exception);
        }
    }
}

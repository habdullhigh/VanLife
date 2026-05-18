using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using VanLife.Api.Data;
using VanLife.Api.Services;
using VanLife.Api.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

public class ImageTests
{
    [Fact]
    public async Task UploadFile_SavesImageRecord()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("imgdb").Options;
        await using var db = new AppDbContext(options);
        var env = new FakeEnv();
        var service = new ImageService(db, env);

        // create a fake IFormFile
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("hello");
        writer.Flush();
        stream.Position = 0;
        IFormFile file = new FormFile(stream, 0, stream.Length, "file", "photo.jpg");

        var image = await service.Upload(file, null);
        Assert.NotNull(image);
        Assert.True(image.Url.StartsWith("/images/"));
    }

    private class FakeEnv : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Test";
        public string WebRootPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot-test");
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}

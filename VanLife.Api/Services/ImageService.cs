using VanLife.Api.Data;
using VanLife.Api.Models;

namespace VanLife.Api.Services;

public class ImageService(InMemoryStore store)
{
    public ImageAsset Upload(string fileName)
    {
        var image = new ImageAsset
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            Url = $"https://cdn.vanlife.fake/images/{Guid.NewGuid():N}-{fileName}",
            UploadedAt = DateTime.UtcNow
        };

        store.Images.Add(image);
        return image;
    }

    public IEnumerable<ImageAsset> GetAll() => store.Images;

    public bool Delete(Guid id)
    {
        var image = store.Images.FirstOrDefault(i => i.Id == id);
        if (image is null)
        {
            return false;
        }

        store.Images.Remove(image);
        return true;
    }
}


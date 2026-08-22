using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace HomeChef.Api.Tests;

public class ImageEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public ImageEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> RegisterAndGetClientAsync(string role)
    {
        var client = _factory.CreateClient();
        var email = $"img-{Guid.NewGuid():N}@test.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Image",
            lastName = "Tester",
            email,
            password = "Password123",
            role,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private async Task<(HttpClient ChefClient, Guid ChefProfileId)> CreateChefWithProfileAsync()
    {
        var chefClient = await RegisterAndGetClientAsync("Chef");
        var createProfile = await chefClient.PostAsJsonAsync("/api/chefs/me", new
        {
            displayName = $"Img Kitchen {Guid.NewGuid():N}",
            bio = "Photo-friendly kitchen.",
            city = "Lahore",
            area = "Gulberg",
            cuisines = new[] { "Pakistani" },
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);

        using var doc = JsonDocument.Parse(await createProfile.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (chefClient, chefProfileId);
    }

    private static async Task<(byte[] Bytes, string FileName)> CreatePngAsync(int width = 120, int height = 90)
    {
        using var image = new Image<Rgba32>(width, height);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                image[x, y] = new Rgba32((byte)(x * 2 % 256), (byte)(y * 2 % 256), 128);
            }
        }

        await using var stream = new MemoryStream();
        await image.SaveAsync(stream, PngFormat.Instance);
        return (stream.ToArray(), "test.png");
    }

    private static MultipartFormDataContent CreateImageContent(byte[] bytes, string fileName) =>
        new()
        {
            { new ByteArrayContent(bytes), "file", fileName },
        };

    [Fact]
    public async Task ChefPhoto_UploadServeAndClear_Lifecycle()
    {
        var (chefClient, _) = await CreateChefWithProfileAsync();
        var (png, fileName) = await CreatePngAsync();

        // 1. Upload sets photo + thumbnail URLs
        var upload = await chefClient.PostAsync("/api/chefs/me/photo", CreateImageContent(png, fileName));
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);

        using var doc = JsonDocument.Parse(await upload.Content.ReadAsStringAsync());
        var photoUrl = doc.RootElement.GetProperty("data").GetProperty("photoUrl").GetString()!;
        var thumbnailUrl = doc.RootElement.GetProperty("data").GetProperty("photoThumbnailUrl").GetString()!;
        Assert.EndsWith(".webp", photoUrl);
        Assert.EndsWith("_thumb.webp", thumbnailUrl);

        // 2. Stored image is publicly served
        var anonymous = _factory.CreateClient();
        var imageResponse = await anonymous.GetAsync(photoUrl);
        Assert.Equal(HttpStatusCode.OK, imageResponse.StatusCode);
        Assert.Equal("image/webp", imageResponse.Content.Headers.ContentType?.MediaType);

        // 3. Clearing the photo nulls both fields
        var clear = await chefClient.DeleteAsync("/api/chefs/me/photo");
        Assert.Equal(HttpStatusCode.OK, clear.StatusCode);

        using var clearDoc = JsonDocument.Parse(await clear.Content.ReadAsStringAsync());
        Assert.Null(clearDoc.RootElement.GetProperty("data").GetProperty("photoUrl").GetString());
    }

    [Fact]
    public async Task ChefPhoto_InvalidType_Rejected()
    {
        var (chefClient, _) = await CreateChefWithProfileAsync();

        var content = new MultipartFormDataContent
        {
            { new ByteArrayContent([0x00, 0x01, 0x02, 0x03]), "file", "notes.txt" },
        };

        var response = await chefClient.PostAsync("/api/chefs/me/photo", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("IMAGE_INVALID", body);
    }

    [Fact]
    public async Task ChefPhoto_Oversized_Rejected()
    {
        var (chefClient, _) = await CreateChefWithProfileAsync();

        // 6 MB of bytes — above the 5 MB limit, rejected before decoding.
        var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(new byte[6 * 1024 * 1024]), "file", "big.png" },
        };

        var response = await chefClient.PostAsync("/api/chefs/me/photo", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("IMAGE_TOO_LARGE", body);
    }

    [Fact]
    public async Task FoodImage_UploadByOwner_AndForbiddenForOtherChef()
    {
        var (chefClient, _) = await CreateChefWithProfileAsync();

        var createFood = await chefClient.PostAsJsonAsync("/api/chefs/me/foods", new
        {
            name = $"Biryani {Guid.NewGuid():N}",
            description = "Fragrant rice.",
            price = 450,
            isAvailable = true,
        });
        Assert.Equal(HttpStatusCode.Created, createFood.StatusCode);

        using var foodDoc = JsonDocument.Parse(await createFood.Content.ReadAsStringAsync());
        var foodId = Guid.Parse(foodDoc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        // 1. Owner uploads the image
        var (png, fileName) = await CreatePngAsync();
        var upload = await chefClient.PostAsync($"/api/chefs/me/foods/{foodId}/image", CreateImageContent(png, fileName));
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);

        using var doc = JsonDocument.Parse(await upload.Content.ReadAsStringAsync());
        var imageUrl = doc.RootElement.GetProperty("data").GetProperty("imageUrl").GetString()!;
        Assert.EndsWith(".webp", imageUrl);

        // 2. Another chef is forbidden from replacing it
        var (otherChef, _) = await CreateChefWithProfileAsync();
        var forbidden = await otherChef.PostAsync($"/api/chefs/me/foods/{foodId}/image", CreateImageContent(png, fileName));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        // 3. Owner clears the image
        var clear = await chefClient.DeleteAsync($"/api/chefs/me/foods/{foodId}/image");
        Assert.Equal(HttpStatusCode.OK, clear.StatusCode);

        using var clearDoc = JsonDocument.Parse(await clear.Content.ReadAsStringAsync());
        Assert.Null(clearDoc.RootElement.GetProperty("data").GetProperty("imageUrl").GetString());
    }

    [Fact]
    public async Task FoodImage_UnknownFood_Returns404()
    {
        var (chefClient, _) = await CreateChefWithProfileAsync();
        var (png, fileName) = await CreatePngAsync();

        var response = await chefClient.PostAsync(
            $"/api/chefs/me/foods/{Guid.NewGuid()}/image", CreateImageContent(png, fileName));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

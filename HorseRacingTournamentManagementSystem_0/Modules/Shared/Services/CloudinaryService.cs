using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HorseRacingTournamentManagementSystem_0.Modules.Shared.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Shared.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string?> UploadImageAsync(IFormFile file, bool isBanner = false)
    {
        if (file.Length == 0) return null;

        var uploadResult = new ImageUploadResult();

        using (var stream = file.OpenReadStream())
        {
            var transformation = isBanner
                // Keep the complete wide image and only scale it down when necessary.
                ? new Transformation().Width(1920).Height(800).Crop("limit").Quality("auto").FetchFormat("auto")
                : new Transformation().Height(500).Width(500).Crop("fill").Gravity("face");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = transformation
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult.SecureUrl?.ToString();
    }
}

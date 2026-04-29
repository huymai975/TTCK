using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using WebAppBookingBoat.Models;

public class PhotoService
{
    private readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(acc);
    }

    // Thêm tham số folderName để mỗi Controller tự quyết định chỗ lưu
    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file, string folderName)
    {
        var uploadResult = new ImageUploadResult();

        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),

                // Lưu vào folder chỉ định, ví dụ: WebApp/Taus hoặc WebApp/Users
                Folder = $"WebAppBookingBoat/{folderName}",

                // Tự động tối ưu dung lượng và định dạng (WebP/Avif)
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    // Hàm xóa ảnh trên Cloudinary
    // publicId là chuỗi định danh sau dấu / cuối cùng và trước phần mở rộng (.jpg)
    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
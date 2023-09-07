using Labb2.ComputerVision.Models;

namespace Labb2.ComputerVision.Services
{
    public interface IComputerVisionService
    {
        Task<AnalyzeImage> AnalyzeImageAsync(string imageUrl, IFormFile ImageFile, int hight, int widht);
    }
}

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Labb2.ComputerVision.Models
{
    public class AnalyzeImage
    {
        public string ImageUrl { get; set; }
        public IFormFile ImageFile { get; set; }
        public string ThumbnailUrl { get; set; }
        public int Hight { get; set; }
        public int Widht { get; set; }
        public ImageAnalysis ImageAnalysisResult { get; set; }
        public List<LandmarksModel> Landmarks { get; set; }
        
    }
}

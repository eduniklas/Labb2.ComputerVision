using Labb2.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;



namespace Labb2.ComputerVision.Services
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly ILogger<AnalyzeImage> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ComputerVisionService(IConfiguration configuration, ILogger<AnalyzeImage> logger, IWebHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<AnalyzeImage> AnalyzeImageAsync(string imageUrl, IFormFile ImageFile, int hight, int widht)
        {
            try
            {
                string cogSvcEndpoint = _configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = _configuration["CognitiveServiceKey"];
                
                ComputerVisionClient cvClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(cogSvcKey))
                {
                    Endpoint = cogSvcEndpoint,
                };

                // Specify features to be retrieved
                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                    VisualFeatureTypes.Description,
                    VisualFeatureTypes.Tags,
                    VisualFeatureTypes.Categories,
                    VisualFeatureTypes.Brands,
                    VisualFeatureTypes.Objects,
                    VisualFeatureTypes.Adult
                };

                ImageAnalysis results;
                List<LandmarksModel> landmarks = new List<LandmarksModel>();
                AnalyzeImage imageAnalysis = new AnalyzeImage();
                Stream imageStream;

                if (Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute) && ImageFile == null)
                {
                    using(HttpClient client = new HttpClient())
                    {
                        using(Stream stream = await client.GetStreamAsync(imageUrl))
                        {
                            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                            imageStream = new MemoryStream(imageBytes);

                            results = await cvClient.AnalyzeImageInStreamAsync(stream, features);
                            var thumbnail = await cvClient.GenerateThumbnailInStreamAsync(hight, widht, imageStream, true);

                            string fileName = Path.GetFileNameWithoutExtension(imageUrl);
                            string thumbnailFileName = $"{fileName}-thumbnail.png";
                            string path = Path.Combine(_hostEnvironment.WebRootPath, "NewImage", thumbnailFileName);

                            using (Stream thumbnailFile = File.Create(path))
                            {
                                await thumbnail.CopyToAsync(thumbnailFile);
                            }
                            foreach (var category in results.Categories)
                            {
                                if (category.Detail?.Landmarks != null)
                                {
                                    foreach (LandmarksModel landmark in category.Detail.Landmarks)
                                    {
                                        if (!landmarks.Any(item => item.Name == landmark.Name))
                                        {
                                            landmarks.Add(landmark);
                                        }
                                    }
                                }
                            }
                            
                            imageAnalysis.ImageUrl = imageUrl;
                            imageAnalysis.ImageAnalysisResult = results;
                            imageAnalysis.Landmarks = landmarks;
                            imageAnalysis.ThumbnailUrl = thumbnailFileName;
                        }
                    }
                }
                else if(ImageFile != null && ImageFile.Length < 0)
                {
                    using (var stream = ImageFile.OpenReadStream())
                    {
                        
                        var thumbnail = await cvClient.GenerateThumbnailInStreamAsync(hight, widht, stream, true);

                        string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                        string thumbnailFileName = $"{fileName}-thumbnail.png";
                        string path = Path.Combine(_hostEnvironment.WebRootPath, "NewImage", thumbnailFileName);

                        using (Stream thumbnailFile = File.Create(path))
                        {
                            await thumbnail.CopyToAsync(thumbnailFile);
                        }
                        
                        results = await cvClient.AnalyzeImageInStreamAsync(stream, features);
                        
                        foreach (var category in results.Categories)
                        {
                            if (category.Detail?.Landmarks != null)
                            {
                                foreach (LandmarksModel landmark in category.Detail.Landmarks)
                                {
                                    if (!landmarks.Any(item => item.Name == landmark.Name))
                                    {
                                        landmarks.Add(landmark);
                                    }
                                }
                            }
                        }

                        imageAnalysis.ImageUrl = imageUrl;
                        imageAnalysis.ImageAnalysisResult = results;
                        imageAnalysis.Landmarks = landmarks;
                        imageAnalysis.ThumbnailUrl = thumbnailFileName;
                    }
                }

               
                return imageAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error has occured");
                throw;
            }
        }
    }
}

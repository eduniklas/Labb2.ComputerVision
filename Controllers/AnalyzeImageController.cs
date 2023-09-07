using Labb2.ComputerVision.Models;
using Labb2.ComputerVision.Services;
using Microsoft.AspNetCore.Mvc;

namespace Labb2.ComputerVision.Controllers
{
    public class AnalyzeImageController : Controller
    {
        private readonly ILogger<AnalyzeImageController> _logger;
        private readonly IComputerVisionService _computerVisionService;

        public AnalyzeImageController(IComputerVisionService computerVisionService)
        {
            _computerVisionService = computerVisionService;
        }

        public IActionResult Index()
        {
            var model = new AnalyzeImage();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(AnalyzeImage model)
        {
            if (string.IsNullOrEmpty(model.ImageUrl))
            {
                ModelState.AddModelError("ImageUrl", "Please enter an image URL.");
                return View(model);
            }

            try
            {
                var analysisResult = await _computerVisionService.AnalyzeImageAsync(model.ImageUrl, model.ImageFile, model.Hight, model.Widht);

                model = analysisResult;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ImageUrl", "Error processing the image: " + ex.Message);
            }
            return View(model);
        }
    }
}

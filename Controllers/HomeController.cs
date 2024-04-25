using Azure.Storage.Blobs;
using BlobStorageDemo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlobStorageDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
       private string Connectionstring = "Test";
        private string containerName = "data";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
            
        }
        public async Task<IActionResult> DownLoadBlob(string filename)
        {
            BlobClient blobClient = new BlobClient(Connectionstring,containerName,filename);
            await blobClient.DownloadToAsync($"D:\\playground\\blob\\{filename}");
                return View("Index");
        }
        public async Task<IActionResult> DeleteBlob(string filename)
        {
            BlobClient blobClient = new BlobClient(Connectionstring, containerName, filename);
            await blobClient.DeleteAsync();
            return View("Index");
        }
        public async Task<IActionResult> BlobList()
        {
            List<BlobItem> blobItems = new List<BlobItem>();

            BlobServiceClient blobServiceClient = new BlobServiceClient(Connectionstring);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await foreach (Azure.Storage.Blobs.Models.BlobItem item in blobContainerClient.GetBlobsAsync())
            {
                BlobItem blobItem = new BlobItem();
                blobItem.Name = item.Name;
                blobItem.Length = item.Properties.ContentLength;
                blobItems.Add(blobItem);
              }

            
            return PartialView("_myPartialView",blobItems);
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadAsync(IFormFile file)
        {
            var name = file.Name;
             // Check if file is not null and has some content
                if (file != null && file.Length > 0)
                {
                BlobServiceClient blobServiceClient = new BlobServiceClient(Connectionstring);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                bool containerExists = await blobContainerClient.ExistsAsync();

                if (containerExists)
                {
                    var blobClient = blobContainerClient.GetBlobClient(file.FileName);

                    // Upload file to blob
                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    ViewBag.ErrorMessage = "";
                    return View("Index");
                }
                else
                {
                    await blobServiceClient.CreateBlobContainerAsync(containerName);
                    var blobClient = blobContainerClient.GetBlobClient(file.FileName);

                    // Upload file to blob
                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    ViewBag.ErrorMessage = "";
                    return View("Index");

                    // Console.WriteLine($"The blob container '{containerName}' does not exist.");
                }


            }
            else
            {
                ViewBag.ErrorMessage = "Please choose File First";
            }

               return View("Index", "Home"); // Redirect to home page or any other page
            
                     
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

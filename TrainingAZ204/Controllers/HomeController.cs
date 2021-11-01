using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TrainingAZ204.Core;
using TrainingAZ204.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Azure.Messaging.EventGrid;
using Azure;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using System.Collections.Generic;

namespace TrainingAZ204.Controllers
{
    //TODO:
    // - replace WindowsAzure.Storage with Azure.Storage.Blobs&Queue and Azure.Data.Tables
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly TelemetryClient _telemetry;

        private EventGridPublisherClient _eventGridClient;
        private QueueClient _queueClient;
        private TableClient _tableClient;
        private BlobContainerClient _blobClient;

        private bool _useEventGrid;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, TelemetryClient telemetry)
        {
            _logger = logger;
            _configuration = configuration;
            _telemetry = telemetry;
            //TODO: fix appsettings
            //_telemetry.InstrumentationKey = "afdd7460-6cbf-4fa6-8d74-1331c6b08e5a";
        }

        private async Task InitializeAsync()
        {
            var connectionString = _configuration.GetConnectionString("AzureStorageAccount");

            _tableClient = new TableClient(connectionString, "persontable");
            await _tableClient.CreateIfNotExistsAsync();

            _queueClient = new QueueClient(connectionString, "personqueue");
            await _queueClient.CreateIfNotExistsAsync();

            _blobClient = new BlobContainerClient(connectionString, "personblob");
            await _blobClient.CreateIfNotExistsAsync();

            //event grid
            _useEventGrid = _configuration.GetValue<bool>("UseEventGrid");

            var urlString = _configuration.GetValue<string>("AzureEventGridUrl");
            var key = _configuration.GetValue<string>("AzureEventGridKey");
            var url = new Uri(urlString);
            var credential = new AzureKeyCredential(key);
            _eventGridClient = new EventGridPublisherClient(url, credential);
        }
        
        private async Task<HomeViewModel> InitializeViewModelAsync()
        {
            await InitializeAsync();

            var collection = new List<Person>();
            var result = _tableClient.Query<Person>();

            foreach (var item in result)
                collection.Add(item);

            return new HomeViewModel { PersonCollection = collection };
        }

        private async Task<string> CreateImageBlobAsync(IFormFile file, Guid imageId)
        {
            var blockBlob = _blobClient.GetBlobClient(imageId.ToString());

            await blockBlob.UploadAsync(file.OpenReadStream());

            return blockBlob.Uri.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string firstName, string lastName, IFormFile file)
        {
            _telemetry.TrackEvent("NewPerson");

            if (_queueClient == null)
            {
                await InitializeAsync();
            }

            var imageId = Guid.Empty;

            if (file != null)
            {
                imageId = Guid.NewGuid();

                await CreateImageBlobAsync(file, imageId);
            }

            if (_useEventGrid)
            {
                await _eventGridClient.SendEventAsync(new EventGridEvent("person", "person-added", "1.0.0", new { FirstName = firstName, LastName = lastName, ImageId = imageId }));
            }
            else
            {
                var person = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{firstName};{lastName};{imageId}"));
                await _queueClient.SendMessageAsync(person);
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Index(HomeViewModel vm)
        {
            _telemetry.TrackPageView("HomePage");
            if (vm == null || vm.PersonCollection == null)
            {
                vm = await InitializeViewModelAsync();
            }
            
            return View(vm);
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

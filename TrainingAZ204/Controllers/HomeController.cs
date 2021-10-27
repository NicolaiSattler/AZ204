﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingAZ204.Core;
using TrainingAZ204.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TrainingAZ204.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private CloudQueue _queue;
        private CloudTable _table;
        private CloudBlobContainer _blob;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        private async Task InitializeAsync()
        {
            //make sure to make a storage account under the same resource group
            var account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=trainingnicolaistorage;AccountKey=j9yf9yBgZs5qf0QgIE1M+ZTQU7IAXXT+4w9fyQms8xoT1vnsI8r41aakLy9EkK7DxMk7uLMSZvpMCchAaMJyng==;EndpointSuffix=core.windows.net");
            var queueClient = account.CreateCloudQueueClient();
            var tableClient = account.CreateCloudTableClient();
            var blobClient = account.CreateCloudBlobClient();
            //queuename must be lowercase 
            _queue = queueClient.GetQueueReference("personqueue");
            //tablename must be lowercase
            _table = tableClient.GetTableReference("persontable");
            _blob = blobClient.GetContainerReference("personblob");

            await _queue.CreateIfNotExistsAsync();
            await _table.CreateIfNotExistsAsync();
            await _blob.CreateIfNotExistsAsync();
        }
        private async Task<HomeViewModel> InitializeViewModelAsync()
        {
            await InitializeAsync();

            var query = new TableQuery<Person>();
            var token = new TableContinuationToken();
            var result = await _table.ExecuteQuerySegmentedAsync(query, token);

            return new HomeViewModel
            {
                PersonCollection = result.Results
            };
        }
        private async Task<string> CreateImageBlobAsync(IFormFile file, Guid imageId)
        {
            var blockBlob = _blob.GetBlockBlobReference(imageId.ToString());

            await blockBlob.UploadFromStreamAsync(file.OpenReadStream());

            return blockBlob.Uri.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string firstName, string lastName, IFormFile file)
        {
            if (_queue == null)
            {
                await InitializeAsync();
            }

            var imageId = Guid.Empty;

            if (file != null)
            {
                imageId = Guid.NewGuid();

                await CreateImageBlobAsync(file, imageId);
            }

            var person = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{firstName};{lastName};{imageId}"));
            var message = new CloudQueueMessage(person);

            await _queue.AddMessageAsync(message);

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Index(HomeViewModel vm)
        {
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

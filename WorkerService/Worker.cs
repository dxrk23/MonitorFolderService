using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _watcher;
        private readonly string _path = @"/Users/DXRK/Documents/worker";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _watcher = new FileSystemWatcher {Path = _path};

            _watcher.Created += OnChanged;
            //watcher.Created += OnCreated;
            //watcher.Changed += OnChanged;
            //watcher.Deleted += OnDeleted;
            //watcher.Renamed += OnRenamed;

            return base.StartAsync(cancellationToken);
        }

        public async Task SendMessage(string filename)
        {
            var message = new
            {
                Type = "email",
                JsonContent = "A file " + filename + " was added."
            };

            var json = JsonConvert.SerializeObject(message);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync("https://localhost:5001/api/queue/add", data);
                string result = response.Content.ReadAsStringAsync().Result;
                _logger.LogInformation(result);
            }
        }


        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("New message sending at : {time}", DateTimeOffset.Now);
            Task sendMessage;
            sendMessage = SendMessage(e.FullPath);
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!Directory.Exists(_path))
                {
                    Directory.CreateDirectory(_path);
                }
                _watcher.EnableRaisingEvents = true;
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);
            }
        }


    }
}

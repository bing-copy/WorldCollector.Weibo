using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskQueue;
using TaskQueue.CommonTaskQueues.DownloadTaskQueue;
using WorldCollector.Weibo.Collectors.AlbumImageCollector.TaskQueues;

namespace WorldCollector.Weibo.Collectors.AlbumImageCollector
{
    public class WeiboImageCollector : TaskQueuePool
    {
        private readonly WeiboImageCollectorOptions _options;
        private const string ProxyPurpose = "WeiboImage";

        public WeiboImageCollector(WeiboImageCollectorOptions options, ILoggerFactory loggerFactory) : base(
            new TaskQueuePoolOptions
            {
                MaxThreads = options.MaxThreads,
                MinInterval = options.MinInterval
            }, loggerFactory)
        {
            _options = options;
        }

        public override async Task Start()
        {
            Add(new WeiboGetPhotoListTaskQueue(new WeiboGetPhotoListTaskQueueOptions
            {
                UrlTemplate = _options.ListUrlTemplate,
                Interval = _options.ListInterval,
                MaxThreads = _options.ListThreads,
                Purpose = ProxyPurpose,
                HttpClientProviderDbConnectionString = _options.HttpClientProviderDbConnectionString,
            }, LoggerFactory));

            Add(new DownloadImageTaskQueue(new DownloadImageTaskQueueOptions
            {
                Interval = _options.DownloadInterval,
                MaxThreads = _options.DownloadThreads,
                DownloadPath = _options.DownloadPath,
                Purpose = ProxyPurpose,
                HttpClientProviderDbConnectionString = _options.HttpClientProviderDbConnectionString,
            }, LoggerFactory));

            Enqueue(new WeiboGetPhotoListTaskData {Page = 1});

            await base.Start();
        }
    }
}
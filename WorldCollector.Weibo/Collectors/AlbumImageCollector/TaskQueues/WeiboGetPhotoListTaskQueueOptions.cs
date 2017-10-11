using TaskQueue.CommonTaskQueues.SpiderTaskQueue;

namespace WorldCollector.Weibo.Collectors.AlbumImageCollector.TaskQueues
{
    public class WeiboGetPhotoListTaskQueueOptions : SpiderTaskQueueOptions
    {
        public string UrlTemplate { get; set; }
    }
}

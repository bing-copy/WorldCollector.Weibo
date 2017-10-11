
using TaskQueue;

namespace WorldCollector.Weibo.Collectors.AlbumImageCollector.TaskQueues
{
    public class WeiboGetPhotoListTaskData : TaskData
    {
        public string SinceId { get; set; }
        public int Page { get; set; }
    }
}

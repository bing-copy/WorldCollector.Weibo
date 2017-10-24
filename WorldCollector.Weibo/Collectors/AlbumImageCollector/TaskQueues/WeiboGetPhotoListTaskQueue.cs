using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsQuery;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TaskQueue;
using TaskQueue.CommonTaskQueues.DownloadTaskQueue;
using TaskQueue.CommonTaskQueues.SpiderTaskQueue;

namespace WorldCollector.Weibo.Collectors.AlbumImageCollector.TaskQueues
{
    public class
        WeiboGetPhotoListTaskQueue : SpiderTaskQueue<WeiboGetPhotoListTaskQueueOptions, WeiboGetPhotoListTaskData>
    {
        public WeiboGetPhotoListTaskQueue(WeiboGetPhotoListTaskQueueOptions options, ILoggerFactory loggerFactory) : base(options, loggerFactory)
        {
        }

        protected override async Task<List<TaskData>> ExecuteAsyncInternal(WeiboGetPhotoListTaskData taskData)
        {
            var url = string.Format(Options.UrlTemplate, taskData.SinceId, taskData.Page,
                (DateTime.Now.ToUniversalTime() - new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc)).Milliseconds);
            var responseString = await (await GetHttpClient()).GetStringAsync(url);
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
            var data = json["data"];
            if (!string.IsNullOrEmpty(data))
            {
                var html = new CQ(data);
                var imageData = html[".photo_cont>a.ph_ar_box>img.photo_pict"].Select(t => t.GetAttribute("src"))
                    .Where(t => !string.IsNullOrEmpty(t)).Select(t =>
                    {
                        var r = Regex.Replace(t, "/thumb\\d+?/", "/large/");
                        if (r.StartsWith("//"))
                        {
                            r = $"https:{r}";
                        }
                        var index = r.IndexOf('?');
                        if (index > -1)
                        {
                            r = r.Substring(0, index);
                        }
                        return r;
                    });
                var imageDownloadTaskData = imageData.Select(a => (TaskData) new DownloadImageTaskData
                {
                    Url = a,
                    RelativeFilename = Path.GetFileName(a)
                }).ToList();
                var nextPageData = html["div[node-type='loading']"].Attr("action-data");
                if (!string.IsNullOrEmpty(nextPageData))
                {
                    var sinceIdMatch = Regex.Match(nextPageData, "since_id=(?<sinceId>[^&]+)");
                    if (sinceIdMatch.Success)
                    {
                        var sinceId = sinceIdMatch.Groups["sinceId"].Value;
                        imageDownloadTaskData.Insert(0,
                            new WeiboGetPhotoListTaskData {Page = taskData.Page + 1, SinceId = sinceId});
                    }
                }
                return imageDownloadTaskData;
            }
            return null;
        }
    }
}
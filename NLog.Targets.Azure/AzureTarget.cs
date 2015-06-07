using System;
using System.Collections.Generic;
using System.Linq;
using AzureStorageExtensions;
using Base64Url;
using NLog.Common;
using NLog.Config;

namespace NLog.Targets
{
    [Target("azure")]
    public class AzureTarget : Target
    {
        [RequiredParameter]
        public string ConnectionName { get; set; }
        [RequiredParameter]
        public string TableName { get; set; }
        public Period Period { get; set; }
        public int RemoveAfter { get; set; }
        public bool SortAscending { get; set; }

        readonly List<AzureProperty> _properties = new List<AzureProperty>();
        [ArrayParameter(typeof(AzureProperty), "property")]
        public IList<AzureProperty> Properties
        {
            get { return _properties; }   
        }
        
        SettingAttribute _setting;
        CloudClient _client;
        protected override void InitializeTarget()
        {
            _setting = new SettingAttribute
            {
                Name = this.TableName,
                Period = this.Period,
                RemoveAfter = this.RemoveAfter,
            };
            _client = CloudClient.Get(this.ConnectionName);
            base.InitializeTarget();
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            try
            {
                var table = _client.GetGenericCloudTable<LogItem>(_setting.Name, _setting);
                var key = TimeId.GetTimeId(DateTime.UtcNow, this.SortAscending);
                var items = logEvents.Select(log => CreateLogItem(key, log.LogEvent));
                table.BulkInsert(items, true);
                foreach (var info in logEvents)
                    info.Continuation(null);
            }
            catch (Exception ex)
            {
                foreach (var info in logEvents)
                    info.Continuation(ex);
            }
        }
        protected override void Write(LogEventInfo logEvent)
        {
            var table = _client.GetGenericCloudTable<LogItem>(_setting.Name, _setting);
            var key = TimeId.GetTimeId(DateTime.UtcNow, this.SortAscending);
            var logItem = CreateLogItem(key, logEvent);
            table.Insert(logItem, true);
        }

        LogItem CreateLogItem(string key, LogEventInfo logEventInfo)
        {
            var logItem = new LogItem
            {
                PartitionKey = key,
                RowKey = TimeId.NewSortableId(logEventInfo.TimeStamp, this.SortAscending),
                LogDateTime = logEventInfo.TimeStamp,
            };
            foreach (var property in _properties)
                logItem[property.Name] = property.Value.Render(logEventInfo);
            return logItem;
        }
    }
}

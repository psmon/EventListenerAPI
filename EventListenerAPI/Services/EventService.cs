using EFCore.BulkExtensions;

using EventListenerAPI.Models;

namespace EventListenerAPI.Services
{
    public class EventService
    {
        private readonly AppDbContext _appDbContext;  
        private DateTime Start;  
        private TimeSpan TimeSpan; 

        public EventService(AppDbContext appDbContext)  
        {  
            _appDbContext = appDbContext;  
        }
        


        #region Add Test For Bulk Insert  
        public async Task<TimeSpan> AddBulkDataAsync(int repeat)  
        {  
            List<EventLog> eventLogs = new(); // C# 9 Syntax.  
            Start = DateTime.Now;  
            for (int i = 0; i < repeat; i++)  
            {  
                eventLogs.Add(new EventLog()  
                {  
                    uuid = Guid.NewGuid().ToString(),
                    event_type = "test1",
                    event_action = "test2",
                    etc_num1 = i,
                    etc_num2 = i+1,
                    etc_num3 = i+2,
                    etc_str1 = "test3",
                    etc_str2 = "test4",
                    etc_str3 = "test5",
                    event_ver = "1.0.0",
                    user_ip = "127.0.0.1",
                    upd_dt = Start
                });  
            }  
            await _appDbContext.BulkInsertAsync(eventLogs);  
            TimeSpan = DateTime.Now - Start;  
            return TimeSpan;
        }  
        #endregion 
    }
}

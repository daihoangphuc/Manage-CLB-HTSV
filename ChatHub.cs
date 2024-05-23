using System.IO;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // Thêm thư viện này

namespace website_CLB_HTSV
{
    public class ChatHub : Hub
    {
        private readonly string _chatHistoryPath;

        // Sử dụng IWebHostEnvironment để lấy đường dẫn tới wwwroot
        public ChatHub(IWebHostEnvironment env)
        {
            // Xây dựng đường dẫn đầy đủ tới file trong thư mục wwwroot/messages
            _chatHistoryPath = Path.Combine(env.WebRootPath, "messages", "chatHistory.txt");
        }

        public override async Task OnConnectedAsync()
        {
            // Gửi lịch sử chat tới người dùng khi họ kết nối
            if (File.Exists(_chatHistoryPath))
            {
                string history = File.ReadAllText(_chatHistoryPath);
                await Clients.Caller.SendAsync("LoadHistory", history);
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string user, string message)
    {
        // Lấy timestamp hiện tại
        var timestamp = TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("dd-MM-yyyy HH:mm:ss");
        
        // Định dạng tin nhắn với timestamp, người dùng và tin nhắn
        string formattedMessage = $"{user}: {message} | {timestamp}\n";

        // Xác định thư mục của file và tạo nếu chưa tồn tại
        var directory = Path.GetDirectoryName(_chatHistoryPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Ghi tin nhắn vào file.
        await File.AppendAllTextAsync(_chatHistoryPath, formattedMessage);

        // Gửi tin nhắn đến tất cả người dùng kết nối
        await Clients.All.SendAsync("ReceiveMessage", user, message, timestamp);
    }
    }
}
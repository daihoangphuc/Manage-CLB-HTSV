using Microsoft.AspNetCore.Mvc;

namespace website_CLB_HTSV.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }



        // POST: Chat/ClearHistory
        [HttpPost]
        public IActionResult ClearHistory()
        {
            // Đường dẫn đến file chatHistory.txt
            string filePath = Path.Combine("wwwroot", "messages", "chatHistory.txt");

            try
            {
                // Mở tập tin để ghi và xóa nội dung
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.Write("");
                }

                ViewBag.Message = "Nội dung của tập tin đã được xóa.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Đã xảy ra lỗi: " + ex.Message;
            }

            // Hiển thị lại view Index hoặc trả về JSON response
            return RedirectToAction("Index");
        }
    }
}

namespace website_CLB_HTSV
{
    public class SessionTimeoutMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTimeoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra xem request hiện tại có phải là trang đăng nhập không và có query parameter "ReturnUrl" hay không
            var isLoginPage = context.Request.Path.StartsWithSegments("/Identity/Account/Login");
            var hasReturnUrl = context.Request.Query.ContainsKey("ReturnUrl");

            if (isLoginPage && hasReturnUrl)
            {
                context.Session.SetString("SessionExpired", "Phiên làm việc của bạn đã hết hạn. Vui lòng đăng nhập lại.");
            }

            await _next(context);
        }
    }
}

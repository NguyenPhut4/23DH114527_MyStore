using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace NguyenTanPhu_23DH114527.Models
{
    // Xác thực bằng Forms Authentication; vai trò (Customer, Staff, Admin) lưu trong vé xác thực
    public static class Auth
    {
        public const int HashCost = 11;

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, HashCost);
        }

        public static bool Verify(string password, string hash)
        {
            try
            {
                return !string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception)
            {
                return false;   // chuỗi băm không hợp lệ
            }
        }

        public static void SignIn(HttpResponseBase response, User user, bool persist)
        {
            var ticket = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddMinutes(60), persist, user.Role);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)) { HttpOnly = true };
            if (persist)
            {
                cookie.Expires = ticket.Expiration;
            }
            response.Cookies.Add(cookie);
        }

        // Gọi trong Application_PostAuthenticateRequest: gắn vai trò từ vé xác thực vào User hiện tại
        public static void AttachRoles(HttpContext context)
        {
            var cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                return;
            }
            try
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket == null || ticket.Expired)
                {
                    return;
                }
                context.User = new GenericPrincipal(new FormsIdentity(ticket), new[] { ticket.UserData });
            }
            catch (Exception)
            {
                // vé không hợp lệ: coi như chưa đăng nhập
            }
        }
    }
}

using System;
using System.Collections.Concurrent;

namespace NguyenTanPhu_23DH114527.Models
{
    // Chống dò mật khẩu: sai 5 lần liên tiếp thì khóa đăng nhập tài khoản đó 5 phút (theo tên đăng nhập + IP).
    // Lưu trong bộ nhớ ứng dụng nên tự xóa khi khởi động lại; đủ cho quy mô đồ án.
    public static class LoginThrottle
    {
        private const int MaxFailures = 5;
        private static readonly TimeSpan LockTime = TimeSpan.FromMinutes(5);
        private static readonly ConcurrentDictionary<string, Entry> Entries = new ConcurrentDictionary<string, Entry>();

        private class Entry
        {
            public int Failures;
            public DateTime LockedUntil;
        }

        private static string Key(string username, string ip)
        {
            return (username ?? "").Trim().ToLowerInvariant() + "|" + ip;
        }

        // Trả về số phút còn bị khóa (0 nếu được phép đăng nhập)
        public static int MinutesLocked(string username, string ip)
        {
            Entry e;
            if (Entries.TryGetValue(Key(username, ip), out e) && e.LockedUntil > DateTime.Now)
            {
                return (int)Math.Ceiling((e.LockedUntil - DateTime.Now).TotalMinutes);
            }
            return 0;
        }

        public static void Fail(string username, string ip)
        {
            var e = Entries.GetOrAdd(Key(username, ip), k => new Entry());
            lock (e)
            {
                if (e.LockedUntil != default(DateTime) && e.LockedUntil <= DateTime.Now)
                {
                    e.Failures = 0;
                    e.LockedUntil = default(DateTime);
                }
                e.Failures++;
                if (e.Failures >= MaxFailures)
                {
                    e.LockedUntil = DateTime.Now.Add(LockTime);
                }
            }
        }

        public static void Reset(string username, string ip)
        {
            Entry removed;
            Entries.TryRemove(Key(username, ip), out removed);
        }
    }
}

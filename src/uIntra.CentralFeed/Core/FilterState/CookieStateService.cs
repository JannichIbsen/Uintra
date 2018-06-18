using System;
using System.Web;
using Uintra.Core;
using Uintra.Core.Extensions;

namespace Uintra.CentralFeed
{
    public abstract class CookieStateService<T> : IStateService<T>
    {
        protected abstract string StateCookieName { get; }

        private readonly ICookieProvider _cookieProvider;

        protected CookieStateService(ICookieProvider cookieProvider)
        {
            _cookieProvider = cookieProvider;
        }

        public void Save(T stateModel)
        {
            var cookie = _cookieProvider.Get(StateCookieName);
            cookie.Value = stateModel.ToJson();
            _cookieProvider.Save(cookie);
        }

        public T Get()
        {
            var cookie = _cookieProvider.Get(StateCookieName);
            if (string.IsNullOrEmpty(cookie?.Value))
            {
                cookie = new HttpCookie(StateCookieName)
                {
                    Expires = DateTime.Now.AddDays(7),
                    Value = GetDefaults().ToJson()
                };
                _cookieProvider.Save(cookie);
            }

            return cookie.Value.Deserialize<T>();
        }

        public abstract T GetDefaults();
    }
}
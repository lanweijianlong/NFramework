﻿using System.Threading;
using System.Web.UI;
using NSoft.NFramework.Tools;
using NSoft.NFramework.Web.PageStatePersisters;

namespace NSoft.NFramework.Caching.Memcached.Web {
    /// <summary>
    /// ASP.NET Web Page 상태 정보를 Memcached 캐시 서버에 저장해주는 클래스입니다. <br/>
    /// 참고 : https://github.com/enyim/EnyimMemcached/wiki/MemcachedClient-Usage
    /// </summary>
    public class MemcachedPageStatePersister : AbstractServerPageStatePersister {
        #region << logger >>

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private static readonly bool IsDebugEnabled = log.IsDebugEnabled;

        #endregion

        public MemcachedPageStatePersister(Page page) : base(page) {}

        private static readonly object _syncLock = new object();
        private static ICacheRepository _cacheRepository;

        /// <summary>
        /// Memcached 캐시 서버를 저장소로 사용하는 Cache Repository 입니다.
        /// </summary>
        public static ICacheRepository CacheRepository {
            get {
                if(_cacheRepository == null)
                    lock(_syncLock)
                        if(_cacheRepository == null) {
                            var cacheReppository = new MemcachedRepository();
                            Thread.MemoryBarrier();
                            _cacheRepository = cacheReppository;
                        }
                return _cacheRepository;
            }
        }

        /// <summary>
        /// ViewState 저장소로부터 저장된 ViewState 정보를 가져옵니다.
        /// </summary>
        protected override void LoadFromRepository() {
            if(IsDebugEnabled)
                log.Debug("캐시에서 키[{0}]에 해당하는 페이지 상태정보를 로드합니다...", StateValue);

            if(StateValue.IsWhiteSpace())
                return;

            var cacheKey = GetCacheKey();

            Pair pair = CacheRepository.Get(cacheKey) as Pair;

            if(pair != null) {
                ViewState = pair.First;
                ControlState = pair.Second;

                if(IsDebugEnabled)
                    log.Debug("Page 상태정보를 저장소로부터 로드했습니다. StateValue=[{0}], CacheKey=[{1}]", StateValue, cacheKey);
            }
            else {
                if(log.IsWarnEnabled)
                    log.Warn("Page 상태정보를 저장소로부터 로드했지만, NULL 값입니다!!! StateValue=[{0}]", StateValue);
            }
        }

        /// <summary>
        /// Page의 ViewState 정보를 특정 저장소에 저장하고, 저장 토큰 값을 <see cref="AbstractServerPageStatePersister.StateValue"/>에 저장합니다.
        /// </summary>
        protected override void SaveToRepository() {
            if(StateValue.IsWhiteSpace())
                StateValue = CreateStateValue();

            if(IsDebugEnabled)
                log.Debug("캐시에 Page 상태정보를 저장합니다... StateValue=[{0}]", StateValue);

            CacheRepository.Set(GetCacheKey(), new Pair(ViewState, ControlState), Expiration);

            if(IsDebugEnabled)
                log.Debug("Membase에 ASP.NET Page 상태정보를 저장했습니다!!! StateValue=[{0}], Expiration=[{1}] (min)", StateValue, Expiration);
        }
    }
}
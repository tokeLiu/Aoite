﻿using Aoite.CommandModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

[assembly: System.Web.PreApplicationStartMethod(typeof(System.Web.WebConfig), "Startup")]
namespace System.Web
{
    /// <summary>
    /// 表示全局配置文件。
    /// </summary>
    public static class WebConfig
    {
        /// <summary>
        /// 启动 ASP.NET。
        /// </summary>
        public static void Startup()
        {
            Web.Mvc.GlobalFilters.Filters.Add(new Web.Mvc.Filters.MvcBasicActionFilterAttribute());
            Webx.Container.Add<IJsonProvider, JsonNetProvider>(); 
        }

        [SingletonMapping]
        class JsonNetProvider : IJsonProvider
        {
            public object Deserialize(string input, Type targetType)
            {
                return JsonConvert.DeserializeObject(input, targetType, WebConfig.DefaultJsonSetting);
            }

            public string Serialize(object obj)
            {
                return JsonConvert.SerializeObject(obj, WebConfig.DefaultJsonSetting);
            }
        }

        #region DefaultJsonSetting

        private static JsonSerializerSettings _DefaultJsonSetting = new JsonSerializerSettings
        {
            //TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormat = Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            //NullValueHandling = NullValueHandling.Ignore,
            //DefaultValueHandling = DefaultValueHandling.Ignore,
            //DateFormatString = "yyyy'-'MM'-'dd HH':'mm':'ss",
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            //Converters = new List<JsonConverter>() { new JsonEnumConverter(), /*new ResultConverter()*/ },

        };

        #endregion

        /// <summary>
        /// 设置或获取默认 JSON 序列化的配置。
        /// <para>默认情况下，忽略空值和默认值，并且支持枚举、属性名首字母自动小写（如 ID=id，IDString=idString，UserName=userName）。</para>
        /// </summary>
        public static JsonSerializerSettings DefaultJsonSetting
        {
            get
            {
                return _DefaultJsonSetting;
            }
            set
            {
                if(value == null) return;
                _DefaultJsonSetting = value;
            }
        }

        /// <summary>
        /// 将指定的对象转换为 JSON 字符串。
        /// </summary>
        /// <param name="value">转换的对象。</param>
        /// <returns>返回一个 JSON 字符串。</returns>
        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(value, WebConfig.DefaultJsonSetting);
        }

        /// <summary>
        /// 表示 ASP.NET 单元测试模拟。
        /// </summary>
        public static class Mocks
        {
            /// <summary>
            /// 创建一个模拟的 MVC 控制器。
            /// </summary>
            /// <typeparam name="TController">控制的数据类型。</typeparam>     
            /// <param name="mockFactoryCallback">模拟的执行器工厂回调函数。</param>
            /// <returns>返回一个控制器的实例。</returns>
            public static TController Create<TController>(Action<MockExecutorFactory> mockFactoryCallback = null)
                      where TController : System.Web.Mvc.Controller, new()
            {
                return Create<TController>(null, mockFactoryCallback);
            }

            /// <summary>
            /// 创建一个模拟的 MVC 控制器。
            /// </summary>
            /// <typeparam name="TController">控制的数据类型。</typeparam>     
            /// <param name="user">当前已授权的登录用户。</param>
            /// <param name="mockFactoryCallback">模拟的执行器工厂回调函数。</param>
            /// <returns>返回一个控制器的实例。</returns>
            public static TController Create<TController>(object user, Action<MockExecutorFactory> mockFactoryCallback = null)
                where TController : Web.Mvc.Controller, new()
            {
                var httpRequest = new HttpRequest(string.Empty, "http://www.aoite.com/", string.Empty);
                var stringWriter = new IO.StringWriter();
                var httpResponce = new HttpResponse(stringWriter);
                var httpContext = new HttpContext(httpRequest, httpResponce);

                var sessionContainer = new SessionState.HttpSessionStateContainer("id", new SessionState.SessionStateItemCollection(),
                                                     new HttpStaticObjectsCollection(), 10, true,
                                                     HttpCookieMode.AutoDetect,
                                                     SessionState.SessionStateMode.InProc, false);

                httpContext.Items["AspSession"] = typeof(SessionState.HttpSessionState).GetConstructor(
                                                         BindingFlags.NonPublic | BindingFlags.Instance,
                                                         null, CallingConventions.Standard,
                                                         new[] { typeof(SessionState.HttpSessionStateContainer) },
                                                         null)
                                                    .Invoke(new object[] { sessionContainer });
                HttpContext.Current = httpContext;

                var appFactoryType = typeof(HttpContext).Assembly.GetType("System.Web.HttpApplicationFactory");

                object appFactory = DynamicFactory.CreateFieldGetter(appFactoryType.GetField("_theApplicationFactory", BindingFlags.NonPublic | BindingFlags.Static))(null);
                DynamicFactory.CreateFieldSetter(appFactoryType.GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance))(appFactory, HttpContext.Current.Application);

                var identityStore = new MockSessionIdentityStore(user);
                var container = ServiceFactory.CreateContainer(identityStore, mockFactoryCallback);
                container.Add<IIdentityStore>(identityStore);
                Webx.Container = container;
                TController c = new TController();
                c.ControllerContext = new Web.Mvc.ControllerContext(new Routing.RequestContext(new HttpContextWrapper(httpContext), new Routing.RouteData()), c);
                return c;
            }

            class MockSessionIdentityStore : IIdentityStore
            {
                public MockSessionIdentityStore(object user)
                {
                    this._User = user;
                }

                private object _User;
                public void Set(object user)
                {
                    this._User = user;
                }

                public object Get()
                {
                    return this._User;
                }

                public void Remove()
                {
                    this._User = null;
                }

                public object GetUser(IIocContainer container)
                {
                    return this.Get();
                }
            }
        }

        /// <summary>
        /// 表示 ASP.NET MVC 的相关配置。
        /// </summary>
        public static class Mvc
        {
            /// <summary>
            /// 在执行操作方法之前发生。
            /// </summary>
            public static event Web.Mvc.ActionExecutingEventHandler ActionExecuting;
            internal static Web.Mvc.ActionResult OnActionExecuting(object sender, Web.Mvc.ActionExecutingContext filterContext, bool allowAnonymous)
            {
                var handler = ActionExecuting;
                if(handler != null)
                {
                    var e = new Web.Mvc.ActionExecutingEventArgs(filterContext, allowAnonymous);
                    handler(sender, e);
                    return e.Result;
                }
                return null;
            }

            /// <summary>
            /// 在执行操作方法之前发生之后。
            /// </summary>
            public static event Web.Mvc.ActionExecutedEventHandler ActionExecuted;
            internal static Web.Mvc.ActionResult OnActionExecuted(object sender, Web.Mvc.ActionExecutedContext filterContext, bool allowAnonymous)
            {
                var handler = ActionExecuted;
                if(handler != null)
                {
                    var e = new Web.Mvc.ActionExecutedEventArgs(filterContext, allowAnonymous);
                    handler(sender, e);
                    return e.Result;
                }
                return null;
            }
        }
    }
}

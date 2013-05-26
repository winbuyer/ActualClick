using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using WidgetSettings;
using System.Net;

namespace WinBuyer.B2B.DealFinder.Core
{
    public class DealFinderContext
    {
        private HttpContext _httpContext;
        private string _domainName;
        private readonly string _style;
        private readonly int _template;
        private string _aid;
        private string _pid;
        private string _sid;
        private string _nid;
        private readonly int _se;
        private string _userId;
        private string _impressionId;
        private string _currency;
        private string _category;
        private readonly bool _abTestStatus;
        private readonly string _abTestId;
        private bool _adMode;
        private readonly int _adId;
        private string _referrer;

        private readonly DataTable _domainInfo;
        private readonly DataTable _campaignInfo;

        private string _abTestGroupId;
        private string _abTestShowWidget;

        private object _result = null;

        public DealFinderContext(HttpContext httpContext)
        {
            _httpContext = httpContext;
            _userId = GetUserId();
            _impressionId = Guid.NewGuid().ToString();

            PageType = "product";
            Timers = new List<dynamic>();
            Debug = new List<dynamic>();
            CseExecutionFailure = new List<dynamic>();
            CseNumberOfRawResults = new List<dynamic>();
            CseOptimizationMapping = new List<dynamic>();
        }
        public DealFinderContext(HttpContext httpContext, string domainName, int campaignId, string style, int template,
            string aid, string pid, string sid, string nid, int se, string op, string category, string categories,
                DataTable domainInfo, DataTable campaignInfo, bool adMode, int adId, string referrer)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            _adId = adId;
            _adMode = adMode;
            _httpContext = httpContext;
            _domainName = domainName;
            _style = style;
            _template = template;
            _aid = aid;
            _pid = pid;
            _sid = sid;
            _nid = nid;
            _domainInfo = domainInfo;
            _campaignInfo = campaignInfo;
            _userId = GetUserId();
            _impressionId = Guid.NewGuid().ToString();
            _currency = CalculateCurrency();
            _category = category;
            _abTestStatus = GetAbTestStatus();
            _abTestId = GetAbTestId();
            _referrer = referrer;
        }

        public DealFinderContext(HttpContext httpContext, string domainName, string campaignId, string category, string currency, string impressionId)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            this.CampaignId = campaignId;

            _httpContext = httpContext;
            _userId = GetUserId();
            _impressionId = impressionId;
            _domainName = domainName;
            _currency = currency;
            _category = category;
        }

        public HttpContext HttpContext
        {
            get
            {
                return _httpContext;
            }
            set
            {
                _httpContext = value;
            }
        }
        public string Ip
        {
            get
            {
                if (_httpContext == null)
                    return "not web environment";

                return !string.IsNullOrEmpty(_httpContext.Request.Headers["rlnclientipaddr"])
                                ? _httpContext.Request.Headers["rlnclientipaddr"]
                                : _httpContext.Request.UserHostAddress;
            }
        }
        public string HostName
        {
            get
            {
                return Dns.GetHostName();
            }
        }
        public string Referrer
        {
            get
            {
                if (_httpContext == null)
                    return "not web environment";

                if (!string.IsNullOrEmpty(_referrer) && _referrer != "none")
                    return _referrer;

                if (_httpContext.Request.UrlReferrer != null)
                    return _httpContext.Request.UrlReferrer.AbsoluteUri;

                return null;
            }
            set
            {
                _referrer = value;
            }
        }
        public string UserAgent
        {
            get
            {
                if (_httpContext == null)
                    return "not web environment";

                return _httpContext.Request.UserAgent;
            }
        }
        public string RequestUrl
        {
            get
            {
                if (_httpContext == null)
                    return "not web environment";

                return _httpContext.Request.Url.AbsoluteUri;
            }
        }
        public string DomainName
        {
            get
            {
                return _domainName;
            }
            set
            {
                _domainName = value;
            }
        }
        public string SearchDomainName
        {
            get;
            set;
        }
        public bool IsDomainInDataBase
        {
            get;
            set;
        }
        public string CampaignId
        {
            get;
            set;
        }
        public string Style
        {
            get
            {
                return _style;
            }
        }
        public int Template
        {
            get
            {
                return _template;
            }
        }
        public string Aid
        {
            get
            {
                return _aid;
            }
            set
            {
                _aid = value;
            }
        }
        public string Pid
        {
            get
            {
                return _pid;
            }
            set
            {
                _pid = value;
            }
        }
        public string Sid
        {
            get
            {
                return _sid;
            }
            set
            {
                _sid = value;
            }
        }
        public string Nid
        {
            get
            {
                return _nid;
            }
            set
            {
                _nid = value;
            }
        }
        public int Se
        {
            get
            {
                return _se;
            }
        }
        public string Op
        {
            get;
            set;
        }
        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
            }
        }
        public string Country
        {
            get;
            set;
        }
        public string PageType
        {
            get;
            set;
        }
        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
            }
        }
        public string ImpressionId
        {
            get
            {
                return _impressionId;
            }
            set
            {
                _impressionId = value;
            }
        }
        public string Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
            }
        }
        public string AbTestGroupId
        {
            get
            {
                return _abTestGroupId;
            }
            set
            {
                _abTestGroupId = value;
            }

        }
        public string AbTestShowWidget
        {
            get
            {
                return _abTestShowWidget;
            }
            set
            {
                _abTestShowWidget = value;
            }
        }
        public bool AbTestStatus
        {
            get
            {
                return _abTestStatus;
            }
        }
        public string AbTestId
        {
            get
            {
                return _abTestId;
            }
        }
        public bool IsAdvertisingMode
        {
            get
            {
                if (!string.IsNullOrEmpty(_aid) && !string.IsNullOrEmpty(_pid))
                    return true;

                return false;
            }
        }
        public bool AdMode
        {
            get
            {
                return _adMode;
            }
            set
            {
                _adMode = value;
            }
        }
        public int AdId
        {
            get
            {
                return _adId;
            }
        }
        public double Price
        {
            get;
            set;
        }
        public bool IsDefaultPrice
        {
            get;
            set;
        }
        public string ProductPriceMatchedBy
        {
            get;
            set;
        }
        public string ProductNameMatchedBy
        {
            get;
            set;
        }
        public string Url
        {
            get;
            set;
        }
        public string Upc
        {
            get;
            set;
        }
        public string Isbn
        {
            get;
            set;
        }
        public string CachedValue
        {
            get;
            set;
        }
        public string CachedValuePriority
        {
            get;
            set;
        }
        public string ProductName
        {
            get;
            set;
        }
        public string Mfg
        {
            get;
            set;
        }
        public bool QaMode
        {
            get;
            set;
        }
        public string RtStack
        {
            get;
            set;
        }
        public string RtPointer
        {
            get;
            set;
        }
        public string RtShowCount
        {
            get;
            set;
        }
        public string RtLastShown
        {
            get;
            set;
        }
        public int Status
        {
            get;
            set;
        }
        public List<dynamic> Timers
        {
            get;
            set;
        }
        public List<dynamic> Debug
        {
            get;
            set;
        }
        public List<dynamic> CseNumberOfRawResults
        {
            get;
            set;
        }
        public List<dynamic> CseOptimizationMapping
        {
            get;
            set;
        }
        public List<dynamic> CseExecutionFailure
        {
            get;
            set;
        }
        public string NewUserId
        {
            get;
            set;
        }
        public string NewImpressionId
        {
            get;
            set;
        }
        public bool IsExistInPcm
        {
            get;
            set;
        }
        public bool IsGetSiteDataFailed
        {
            get;
            set;
        }
        public bool IsGetSiteDataExecuted
        {
            get;
            set;
        }
        public int TotalNumberOfOffersToDisplay
        {
            get;
            set;
        }
        public string CurrentTrigger
        {
            get;
            set;
        }
        public string DomainPriceRule
        {
            get;
            set;
        }
        public bool CookiesEnabled
        {
            get;
            set;
        }

        public void AddTimer(string key, string value)
        {
            Timers.Add(new
            {
                name = key,
                time = value
            });
        }
        public void AddDebug(string key, string value)
        {
            Debug.Add(new
            {
                key = key,
                value = value
            });
        }
        public void AddCseNumberOfRawResults(string key, string value)
        {
            CseNumberOfRawResults.Add(new
            {
                key = key,
                value = value
            });
        }
        public void AddCseOptimizationMapping(string key, string value)
        {
            CseOptimizationMapping.Add(new
            {
                key = key,
                value = value
            });
        }
        public void AddCseExecutionFailure(string key, string value)
        {
            CseExecutionFailure.Add(new
            {
                key = key,
                value = value
            });
        }

        public object Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }

        public void AddCookies()
        {
            var activityCookie = new HttpCookie("OCPActivity", "GUID=" + _userId);

            activityCookie.Expires = DateTime.Now.AddYears(10);
            activityCookie.Domain = "winbuyer.com";
            activityCookie.Path = "/";

            var impressionCookie = new HttpCookie(string.Format("wb_{0}_ImpressionId", _domainName), _impressionId);

            impressionCookie.Expires = DateTime.Now.AddYears(10);
            impressionCookie.Domain = "winbuyer.com";
            impressionCookie.Path = "/";

            _httpContext.Response.Cookies.Add(activityCookie);
            _httpContext.Response.Cookies.Add(impressionCookie);

            activityCookie = new HttpCookie("OCPActivity", "GUID=" + _userId);

            activityCookie.Expires = DateTime.Now.AddYears(10);
            activityCookie.Domain = "actualclick.com";
            activityCookie.Path = "/";

            impressionCookie = new HttpCookie(string.Format("wb_{0}_ImpressionId", _domainName), _impressionId);

            impressionCookie.Expires = DateTime.Now.AddYears(10);
            impressionCookie.Domain = "actualclick.com";
            impressionCookie.Path = "/";

            _httpContext.Response.Cookies.Add(activityCookie);
            _httpContext.Response.Cookies.Add(impressionCookie);
        }
        
        private string GetAbTestId()
        {
            return _domainInfo.Rows[0]["AB_Test_ID"] == DBNull.Value ? null : (string)_domainInfo.Rows[0]["AB_Test_ID"];
        }
        private bool GetAbTestStatus()
        {
            return _domainInfo.Rows[0]["AB_Test_Status"] == DBNull.Value ? false : (bool)_domainInfo.Rows[0]["AB_Test_Status"];
        }
        private string GetUserId()
        {
            if (_httpContext == null)
                return null;

            if (_httpContext.Request.Cookies["OCPActivity"] == null)
            {
                return Guid.NewGuid().ToString();
            }
            else
            {
                return HttpUtility.UrlDecode(_httpContext.Request.Cookies["OCPActivity"].Value).Replace("GUID=", "");
            }
        }
        private string CalculateCurrency()
        {
            string currency = "$";

            switch (((string)_campaignInfo.Rows[0]["ShoppinApiDest"]).ToLower())
            {
                case "fr":
                case "de":
                    currency = "€";
                    break;
                case "uk":
                    currency = "£";
                    break;
                default:
                    currency = "$";
                    break;
            }

            return currency;
        }

        public bool IsRetargetingResult
        {
            get;
            set;
        }
        public bool IsEcommerceDomain
        {
            get;
            set;
        }
        public bool IsRetargetingEnabled
        {
            get;
            set;
        }
        public string EcommerceRecency
        {
            get;
            set;
        }
        public string NonEcommerceRecency
        {
            get;
            set;
        }
        public string NonEcommerceFrequency
        {
            get;
            set;
        }
    }
}


/// <reference path="/js/jquery-1.4.4-vsdoc.js" />
(function ()
{
    var _actualClickGateway = {};

    _actualClickGateway.init = function ()
    {
        _addScript('http://actualclick.apphb.com/static/get-js?name=actualclick.data.min.js', function ()
        {
            var jq = jQuery.noConflict(true);

            _addCss('http://actualclick.apphb.com/static/get-css?name=actualclick.min.css', function ()
            {
                _init(jq);
            });
        });
    }

    var _init = function (jQuery)
    {
        actualClick.init(
        {
            campaignId: '$campaignId$',
            sid: '$sid$',
            qaMode: '$qaMode$'
        }, jQuery);
    }

    var _addScript = function (scriptURL, onloadCB)
    {
        var scriptEl = document.createElement("script");
        scriptEl.type = "text/javascript";
        scriptEl.src = scriptURL;

        function calltheCBcmn()
        {
            onloadCB(scriptURL);
        }

        if (typeof (scriptEl.addEventListener) != 'undefined')
        {
            /* The FF, Chrome, Safari, Opera way */
            scriptEl.addEventListener('load', calltheCBcmn, false);
        }
        else
        {
            /* The MS IE 8+ way (may work with others - I dunno)*/
            function handleIeState()
            {
                if (scriptEl.readyState == 'loaded')
                {
                    calltheCBcmn(scriptURL);
                }
            }

            var ret = scriptEl.attachEvent('onreadystatechange', handleIeState);
        }

        document.getElementsByTagName("head")[0].appendChild(scriptEl);
    }

    var _addCss = function (cssURL, onloadCB)
    {
        var scriptEl = document.createElement("link");
        scriptEl.href = cssURL;
        scriptEl.rel = "stylesheet";
        scriptEl.type = "text/css";

        function calltheCBcmn()
        {
            onloadCB(cssURL);
        }

        if (typeof (scriptEl.addEventListener) != 'undefined')
        {
            /* The FF, Chrome, Safari, Opera way */
            scriptEl.addEventListener('load', calltheCBcmn, false);
        }
        else
        {
            /* The MS IE 8+ way (may work with others - I dunno)*/
            function handleIeState()
            {
                if (scriptEl.readyState == 'loaded')
                {
                    calltheCBcmn(scriptURL);
                }
            }

            var ret = scriptEl.attachEvent('onreadystatechange', handleIeState);
        }

        document.getElementsByTagName("head")[0].appendChild(scriptEl);
    }

    var _getProtocolType = function ()
    {
        var url = window.location.href;
        var arr = url.split("/");
        var result = arr[0];

        return result;
    }

    return _actualClickGateway;

} ()).init();

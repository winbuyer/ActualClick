/// <reference path="/js/$-1.4.4-vsdoc.js" />

var actualClick = (function ()
{
    var $ = null;
    var _actualClick = {};
    var _config = null;

    var _qaDataUrl = 'http://actualclick.apphb.com/extentions/get-products-by-trigger';
    var _qaTimersUrl = 'http://actualclick.apphb.com/extentions/log-timers';
    var _qaAboutUrl = 'http://actualclick.apphb.com/extentions/get-about';

    var _dataUrl = 'http://actualclick.apphb.com/extentions/get-products-by-trigger';
    var _timersUrl = 'http://actualclick.apphb.com/extentions/log-timers';
    var _aboutUrl = 'http://actualclick.apphb.com/extentions/get-about';

    _actualClick.init = function (config, jQuery)
    {
        $ = jQuery;
        _config = config;

        //var start = new Date().getTime();

        $.jsonp(
        {
            url: _config.qaMode == 'true' ? _qaDataUrl : _dataUrl,
            cache: false,
            data:
            {
                url: encodeURIComponent(document.location),
                campaignId: _config.campaignId,
                sid: _config.sid,
                responseType: 'json',
                qaMode: _config.qaMode
            },
            callbackParameter: 'callback',
            success: function (response, textStatus)
            {
                if (response.products.length == 0 && _config.qaMode == 'true')
                {
                    var html = _buildHtml(response.html_template, _getFakeProduct(), '#');

                    $.gritter.add({
                        sticky: true,
                        text: html,
                        width: '410px',
                        height: '110px',
                        qaMode: _config.qaMode,
                        onAbout: function ()
                        {
                            if (typeof (con_actualclick) == 'undefined')
                                _openAboutPopUp(_config.qaMode == 'true' ? _qaAboutUrl : _aboutUrl, 'about', 550, 300);
                            else
                                con_actualclick.mam.sendCallbackMessage('WHATS_THIS');
                        },
                        onQa: function ()
                        {
                            var html = _buildQaHtml(response.debug);

                            $(html).bPopup();
                        }
                    });

                    return;
                }

                //                var ajaxTime = new Date().getTime() - start;

                //                response.timers.push({ name: 'global-client-time', time: ajaxTime });

                //                var jsonString = $.toJSON(
                //                {
                //                    timers: response.timers,
                //                    url: document.location.href,
                //                    campaignId: _config.campaignId,
                //                    cachedValue: response.sku
                //                });

                //                var jsonStringEncoded = base64.encode(jsonString);

                //                $.jsonp(
                //                {
                //                    url: _config.qaMode == 'true' ? _qaTimersUrl : _timersUrl,
                //                    cache: false,
                //                    data:
                //                    {
                //                        data: jsonStringEncoded
                //                    },

                //                    callbackParameter: 'callback'
                //                });

                if (response.products.length == 0)
                    return;

                var html = _buildHtml(response.html_template, response.products, response.more_deals_url);

                $.gritter.add({
                    sticky: true,
                    text: html,
                    width: response.width,
                    height: '110px',
                    qaMode: _config.qaMode,
                    onAbout: function ()
                    {
                        if (typeof (con_actualclick) == 'undefined')
                            _openAboutPopUp(_config.qaMode == 'true' ? _qaAboutUrl : _aboutUrl, 'about', 550, 300);
                        else
                            con_actualclick.mam.sendCallbackMessage('WHATS_THIS');
                    },
                    onQa: function ()
                    {
                        var html = _buildQaHtml(response.debug);

                        $(html).bPopup();
                    }
                });
            },
            error: function (xOptions, textStatus)
            {
            }
        });
    };

    var _buildQaHtml = function (data)
    {
        var container = $('<div></div>')
                            .addClass('actual-click-qa');

        for (var i = 0; i < data.length; i++)
        {
            container.append($('<div></div>')
                            .addClass('actual-click-qa-element')
                            .html('<span class="actual-click-qa-element-key">'
                             + data[i].key +
                             '</span>: <span class="actual-click-qa-element-value">'
                             + data[i].value + '</span>'));
        }

        return container;
    }

    var _openAboutPopUp = function (url, name, w, h)
    {
        w += 32;
        h += 96;
        var win = window.open(url, '', 'width=' + w + ', height=' + h + ', ' + 'location=no, menubar=no, ' + 'status=no, toolbar=no, scrollbars=no, resizable=no');
        win.resizeTo(w, h); win.focus();
    }

    var _getFakeProduct = function ()
    {
        var products = [];
        var product =
        {
            product_url: '',
            product_name: '',
            product_image: '',
            merchant_image: '',
            merchant_name: '',
            country: 'US',
            product_price: 0
        };

        products.push(product);

        return products;
    }

    var _buildHtml = function (template, data, moreDealsUrl, currency)
    {
        var dealFinder = $(template).clone();
        var productsContainer = dealFinder.find('.actualclick-products-container').html('');

        dealFinder.find('.actualclick-left-image-container a').attr('href', moreDealsUrl);

        for (var i = 0; i < data.length; i++)
        {
            var product = $(template).find('.actualclick-one-product-container').clone();

            var clickUrl = data[i].product_url;

            product.find('a').attr('href', clickUrl);

            product.find('.actualclick-product-name div').html(data[i].product_name);

            product.find('.actualclick-product-image img').attr('src', data[i].product_image);
            product.find('.actualclick-product-image img').attr('title', data[i].product_name);
            product.find('.actualclick-product-image img').attr('alt', data[i].product_name);

            if (data[i].merchant_image == null || data[i].merchant_image == '')
            {
                product.find('.actualclick-product-merchant-logo').html('<div>' + data[i].merchant_name + '</div>');
            }
            else
            {
                product.find('.actualclick-product-merchant-logo img').attr('src', data[i].merchant_image);
                product.find('.actualclick-product-merchant-logo img').attr('title', data[i].merchant_name);
                product.find('.actualclick-product-merchant-logo img').attr('alt', data[i].merchant_name);
            }

            var currency = data[i].country == 'US' ? '$' : '£';

            if (data[i].product_price > 0)
                product.find('.actualclick-product-price div').html(currency + data[i].product_price);

            productsContainer.append(product);

            if (i < data.length - 1)
                productsContainer.append('<div class="actualclick-product-divider"></div>');
        }

        return dealFinder.html();
    }

    return _actualClick;
} ());
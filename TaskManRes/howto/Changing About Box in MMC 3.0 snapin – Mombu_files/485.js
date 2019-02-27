/**
 * plain js functions for displaying ads by size
 *
 **/

// basic variables
var advanced_ads_resizetimeout = 1000; // time to wait until resized window width is saved, in millisec
var advanced_ads_cookieexpires = 14; // days until the cookie expires

// save browserWidth in a cookie if not set or not equal to current saved width
if ( !advanced_ads_get_cookie('advanced_ads_browser_width') ||
        advanced_ads_get_cookie('advanced_ads_browser_width') !== advanced_ads_get_browser_width()) {

    advanced_ads_save_width();

}

// save new browser width, when window resizes
if (window.addEventListener) {    // most non-IE browsers and IE9
   window.addEventListener("resize", advanced_ads_resize_window, false);
} else if (window.attachEvent) {  // Internet Explorer 5 or above
   window.attachEvent("onresize", advanced_ads_resize_window);
}

function advanced_ads_resize_window () {
	advads_resize_delay(function(){
		advanced_ads_save_width();
	}, advanced_ads_resizetimeout);
}    

// save width in cookie
function advanced_ads_save_width () {
    var width = advanced_ads_get_browser_width();
    advanced_ads_set_cookie('advanced_ads_browser_width', width, advanced_ads_cookieexpires);

}

// create a listener calling only once after resize
// http://stackoverflow.com/questions/2854407/javascript-jquery-window-resize-how-to-fire-after-the-resize-is-completed
var advads_resize_delay = (function(){
    var timer = 0;
    return function (callback, ms) {
	clearTimeout(timer);
	timer = setTimeout(callback, ms);
    };
})();

// thanks to http://www.w3schools.com/js/js_cookies.asp for the code
function advanced_ads_get_cookie(c_name)
{
    var i,x,y,ARRcookies=document.cookie.split(";");
    for (i=0;i<ARRcookies.length;i++)
    {
        x=ARRcookies[i].substr(0,ARRcookies[i].indexOf("="));
        y=ARRcookies[i].substr(ARRcookies[i].indexOf("=")+1);
        x=x.replace(/^\s+|\s+$/g,"");
        if (x==c_name)
        {
            return unescape(y);
        }
    }
}

/**
 * name = cookie name
 * value = cookie value
 * exdays = days until cookie expires
 */
function advanced_ads_set_cookie( name, value, exdays, path, domain, secure)
{
    var exdate=new Date();
    exdate.setDate(exdate.getDate() + exdays);
    document.cookie = name + "=" + escape(value) +
    ((exdate == null) ? "" : "; expires=" + exdate.toUTCString()) +
    ((path == null) ? "; path=/" : "; path=" + path) +
    ((domain == null) ? "" : "; domain=" + domain) +
    ((secure == null) ? "" : "; secure");
}

function advanced_ads_check_cookie(c_name)
{
    var c_value=advanced_ads_get_cookie( c_name );
    if (c_value!=null && c_value!="")
    {
        return false;
    }
    return true;
}

/**
 * get the width of the browser
 */
function advanced_ads_get_browser_width() {
    return jQuery(window).width();
    // deprecated code
    var browserWidth = 0;
    if( typeof( window.innerWidth ) == 'number' ) {
        //Non-IE
        browserWidth = window.innerWidth;
    } else if( document.documentElement && document.documentElement.clientWidth ) {
        //IE 6+ in 'standards compliant mode'
        browserWidth = document.documentElement.clientWidth;
    } else if( document.body && document.body.clientWidth ) {
        //IE 4 compatible
        browserWidth = document.body.clientWidth;
    }
    return browserWidth;
}
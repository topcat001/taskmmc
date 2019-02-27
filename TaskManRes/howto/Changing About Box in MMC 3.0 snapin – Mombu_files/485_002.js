jQuery(document).ready(function($){
    // if cache-busting module is enabled
    if ( typeof advanced_ads_pro !== 'undefined' ) {
        advanced_ads_pro.observers.add( function( event ) {
            // waiting for the moment when all passive cache-busting ads will be inserted into html 
            if ( event.event === 'inject_passive_ads' && $.isArray( event.ad_ids ) ) {
                var advads_ad_ids;
                if ( advadsTracking.method === 'frontend' ) {
                    // cache-busting: off + cache-busting: passive
                    advads_ad_ids = advads_tracking_ads.concat( event.ad_ids );
                } else {
                    // select only passive cache-busting ads
                    advads_ad_ids = event.ad_ids;
                }

                advads_track_ads( advads_ad_ids );
            }
        } );
    } else if ( advadsTracking.method === 'frontend' ) {
        // cache-busting: off
        advads_track_ads( advads_tracking_ads );
    }
});

jQuery( document ).on( 'advads_track_ads', function( e, ad_ids ) {
    advads_track_ads( ad_ids );
});

/**
 * track ads
 *
 * @param {arr} advads_ad_ids
 */
function advads_track_ads( advads_ad_ids ) {
    if ( ! advads_ad_ids.length ) return; // do not send empty array

    var data = {
        ads: advads_ad_ids
    };
    jQuery.post( advadsTracking.ajaxurl, data, function(response) {} );
}
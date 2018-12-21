(function($) {

    "use strict";

   /* Mobile Menu
    * ---------------------------------------------------- */ 
    var clMobileMenu = function() {

        var navWrap = $('.header__nav-wrap'),
            closeNavWrap = navWrap.find('.header__overlay-close'),
            menuToggle = $('.header__toggle-menu'),
            siteBody = $('body');
        
        menuToggle.on('click', function(e) {
            var $this = $(this);

            e.preventDefault();
            e.stopPropagation();
            siteBody.addClass('nav-wrap-is-visible');
        });

        closeNavWrap.on('click', function(e) {
            
            var $this = $(this);
            
            e.preventDefault();
            e.stopPropagation();
        
            if(siteBody.hasClass('nav-wrap-is-visible')) {
                siteBody.removeClass('nav-wrap-is-visible');
            }
        });

        // open (or close) submenu items in mobile view menu. 
        // close all the other open submenu items.
        $('.header__nav .has-children').children('a').on('click', function (e) {
            e.preventDefault();

            if ($(".close-mobile-menu").is(":visible") == true) {

                $(this).toggleClass('sub-menu-is-open')
                    .next('ul')
                    .slideToggle(200)
                    .end()
                    .parent('.has-children')
                    .siblings('.has-children')
                    .children('a')
                    .removeClass('sub-menu-is-open')
                    .next('ul')
                    .slideUp(200);
            }
        });
    };

   /* Initialize
    * ------------------------------------------------------ */
    (function ssInit() {
        clMobileMenu();
    })();
        
})(jQuery);
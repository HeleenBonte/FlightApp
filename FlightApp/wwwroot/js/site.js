// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    // Check current URL to apply the correct background
    const currentPath = window.location.pathname.toLowerCase();

    // Fix footer positioning on all pages
    adjustFooter();
    $(window).on('resize', adjustFooter);

    // Set home-page class for specific styling
    if (currentPath === '/' || currentPath.endsWith('/home') || currentPath.endsWith('/home/index')) {
        $('body').addClass('home-page');
    }

    // Apply flights background
    if (currentPath.includes('/flights')) {
        $('body').addClass('flights-page');

        // Apply styling to search instructions
        $('#content .text-center.text-muted p').addClass('search-instruction');
        $('#content .text-muted').removeClass('text-muted');
    }

    // Apply routes background
    else if (currentPath.includes('/routes') || currentPath.includes('/getroutes')) {
        $('body').addClass('routes-page');

        // Apply styling to search instructions
        $('#content .text-center.text-muted p').addClass('search-instruction');
        $('#content .text-muted').removeClass('text-muted');
    }

    // Apply white background and black text to footer
    $('.footer').removeClass('text-muted').addClass('white-footer');
});

// Function to adjust footer position
function adjustFooter() {
    const windowHeight = $(window).height();
    const bodyHeight = $('body').height();
    const footerHeight = $('.footer').outerHeight();
    const headerHeight = $('header').outerHeight();
    const mainContent = $('main');

    // Set minimum height for main content to push footer to bottom
    const minContentHeight = windowHeight - headerHeight - footerHeight;
    mainContent.css('min-height', minContentHeight + 'px');
}
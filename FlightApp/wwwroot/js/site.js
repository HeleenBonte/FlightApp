// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    // Check current URL to apply the correct background
    const currentPath = window.location.pathname.toLowerCase();

    // Apply flights background
    if (currentPath.includes('/flights')) {
        $('body').addClass('flights-page');
    }

    // Apply routes background
    else if (currentPath.includes('/routes') || currentPath.includes('/getroutes')) {
        $('body').addClass('routes-page');
    }
});


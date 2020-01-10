// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

jQuery.validator.addMethod("enforcetrue", function (value, element, param) {
    return element.checked;
});
jQuery.validator.unobtrusive.adapters.addBool("enforcetrue");
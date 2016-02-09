// This service adds CSS classes to body when something is dragged onto the page
angular.module("EavServices")
    .factory("dragClass", function () {

        document.addEventListener("dragover", function() {
            if(this === document)
                document.body.classList.add("eav-dragging");
        });
        document.addEventListener("dragleave", function() {
            if(this === document)
                document.body.classList.remove("eav-dragging");
        });

        return {};

    });


/* File Type Services
 * Helps check if something is an image (then the UI usually wants a thumbnail)
 * ...or if it has an icon in the font-library - then it can provide the class name for the icon
 */
angular.module("EavServices").service("fileType", function () {
    var svc = {};
    svc.iconPrefix = "eav-icon-";
    svc.defaultIcon = "file";
    svc.checkImgRegEx = /(?:([^:\/?#]+):)?(?:\/\/([^\/?#]*))?([^?#]*\.(?:jpg|jpeg|gif|png))(?:\?([^#]*))?(?:#(.*))?/i;
    svc.extensions = {
        doc: "file-word",
        docx: "file-word",
        xls: "file-excel",
        xlsx: "file-excel",
        ppt: "file-powerpoint",
        pptx: "file-powerpoint",
        pdf: "file-pdf",
        mp3: "file-audio",
        avi: "file-video",
        mpg: "file-video",
        mpeg: "file-video",
        mov: "file-video",
        mp4: "file-video",
        zip: "file-archive",
        rar: "file-archive",
        txt: "file-text",
        html: "file-code",
        css: "file-code",
        xml: "file-code",
        xsl: "file-code",
        vcf: "user"
    };

    svc.getExtension = function(filename) {
        return filename.substr(filename.lastIndexOf(".") + 1).toLowerCase();
    };

    svc.getIconClass = function getClass(filename) {
        return svc.iconPrefix + (svc.extensions[svc.getExtension(filename)] || svc.defaultIcon);
    };
    
    svc.isKnownType = function(filename) {
        return svc.extensions.indexOf[svc.getExtension(filename)] !== -1;
    };

    svc.isImage = function(filename) {
        return svc.checkImgRegEx.test(filename);
    };

    // not used yet, so commented out
    //svc.type = function(url) {
    //    if (svc.isImage(url))
    //        return "image";
    //    return "file";
    //};

    return svc;
});
/* 
 * ANGULAR SET !
 * This is a special grunt-file to build the libraries we use in our system
 * We need it, because the bower-packages often contain too much material
 * so we extract the parts we want and place it in dist/lib
 * 
 * We also merge many files into 1 where we know that we only need them merged
 * to reduce the count of included files
 * 
 */
module.exports = function (grunt) {
    "use strict";
    var angular = {
    	banner: "/*! Angular-Package for 2sic 2sxc & eav <%= grunt.template.today(\"yyyy-mm-dd hh:MM\") %> */\n",
		cwd: "bower_components/",
		jsFiles: [
			// the basic files
            "bower_components/angular/angular.min.js",
			"bower_components/angular-resource/angular-resource.min.js",
            "bower_components/angular-animate/angular-animate.min.js",
            "bower_components/oclazyload/dist/oclazyload.min.js",

			// visual effects etc.
            "bower_components/angular-bootstrap/ui-bootstrap-tpls.min.js",
            "bower_components/angular-toastr/dist/angular-toastr.tpls.min.js",

			// i18n files
			"bower_components/angular-translate/angular-translate.min.js",
			"bower_components/angular-translate-loader-partial/angular-translate-loader-partial.min.js",

			// files used by formly and the general edit UI
			"bower_components/api-check/dist/api-check.min.js",
			"bower_components/angular-base64-upload/dist/angular-base64-upload.min.js",
			"bower_components/angular-formly/dist/formly.min.js",
			"bower_components/angular-formly-templates-bootstrap/dist/angular-formly-templates-bootstrap.min.js",
            "bower_components/angular-ui-tree/dist/angular-ui-tree.min.js",

            // promise-window just to be sure we can use it till all old dialogs have been removed
			"bower_components/promise-window/dist/promise-window.min.js",

            // dropzone for uploads
            "bower_components/dropzone/dist/dropzone.js",

            // switch for toggle-feature
            "bower_components/angular-ui-switch/angular-ui-switch.min.js",
		],
		cssFiles: [
            "bower_components/bootstrap/dist/css/bootstrap.min.css",
            "bower_components/bootflat-for-2sic/bootflat/css/bootflat.min.css",
            "bower_components/angular-ui-tree/dist/angular-ui-tree.min.css",
            "bower_components/angular-ui-switch/angular-ui-switch.min.css",

            // toaster
            "bower_components/angular-toastr/dist/angular-toastr.css"
		],
		fontFiles: [
            "bower_components/bootstrap/fonts/glyphicons-halflings-regular.woff2",
            "bower_components/bootstrap/fonts/glyphicons-halflings-regular.woff",
            "bower_components/bootstrap/fonts/glyphicons-halflings-regular.ttf",
            "bower_components/bootstrap/fonts/glyphicons-halflings-regular.eot",
            "bower_components/bootstrap/fonts/glyphicons-halflings-regular.svc"
		],
        tmp: "tmp/lib/angular/",
        dist: "dist/lib/angular/",
        fonts: "dist/lib/fonts/",
        concatFile: "dist/lib/angular/set.min.js",
        uglifyFile: "dist/lib/angular/set.min.js",
        concatCss: "dist/lib/angular/set.min.css",

    };

    // Project configuration.
    grunt.config.merge({
    	clean: {
    		angSetTmp: angular.tmp + "**/*", 
    		angSetDist: angular.dist + "/*",
    		angSetFonts: angular.fonts
    	},

    	copy: {
    	    angSetFonts: {
                files: [
                    {
                        expand: true,
                        flatten: true,
                        src: angular.fontFiles,
                        dest: angular.fonts
                    }
                ]
            }
        },
        concat: {
            angSetLib: {
        		nonull: true,
                src: angular.jsFiles,
                dest: angular.concatFile
        	},
            angSetCss: {
        	    nonull: true,
        	    src: angular.cssFiles,
        	    dest: angular.concatCss
        	}
        }

    });

    // Default task.
    grunt.registerTask("build-angular-lib", [
        "clean:angSetTmp",
        "clean:angSetDist",
        "clean:angSetFonts",
        "copy:angSetFonts",
        "concat:angSetLib",
        "concat:angSetCss"
    ]);
};
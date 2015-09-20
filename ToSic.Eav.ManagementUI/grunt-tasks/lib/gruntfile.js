/* 
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
            "angular/angular.js",
            "angular-animate/angular-animate.js",
            "angular-bootstrap/ui-bootstrap.js"
        ],
        tmp: "tmp/lib/",
        dist: "dist/lib/",
        concatFile: "dist/lib/angular-set.js",
        uglifyFile: "dist/lib/angular-set.min.js"
    };

    // Project configuration.
    grunt.initConfig({
        copy: {
            angular: {
                files: [
                    {
                        expand: true,
                        cwd: angular.cwd,
                        src: angular.jsFiles,
                        dest: angular.tmp
                    }
                ]
            }
        },
        concat: {
            angular: {
                src: angular.tmp + "**/*.js",
                dest: angular.concatFile
            }
        },


        uglify: {
            options: {
                banner: angular.banner,
                sourceMap: true
            },

            angular: {
                src: angular.concatFile,
                dest: angular.uglifyFile
            }
        },

        compress: {
            main: {
                options: {
                    mode: "gzip"
                },
                expand: true,
                cwd: angular.dist,
                src: ["**/*.min.js"],
                dest: angular.dist,
                ext: ".gz.js"
            }
        }
    });

    // Default task.
    grunt.registerTask("buildAngularLib", [
        //"clean:tmp",
        "copy",
        "concat",
        "uglify",
		"compress"
    ]);
};
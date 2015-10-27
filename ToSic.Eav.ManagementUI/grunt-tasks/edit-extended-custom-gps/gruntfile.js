/*
 * Custom GPS Grunt-File
 * 
 */

module.exports = function (grunt) {
    "use strict";

    var tmpRoot = "tmp/";
    var job = {
        cwd: "src/edit-extended/",
        cwdJs: "src/edit-extended/**/*.js",
        lib: [
            "bower_components/lodash/lodash.min.js",
            "bower_components/angular-google-maps/dist/angular-google-maps.min.js",
            "bower_components/angular-simple-logger/dist/index.js"
        ],
        tmp: "tmp/edit-extended/",
        templates: "tmp/edit-extended/html-templates.js",
        dist: "dist/edit-extended/",
        concatFile: "dist/edit/extensions/field-custom-gps/custom-gps.js",
        uglifyFile: "dist/edit/extensions/field-custom-gps/custom-gps.min.js",
        concatCss: "dist/edit/extensions/field-custom-gps/custom-gps.css",
        concatCssMin: "dist/edit/extensions/field-custom-gps/custom-gps.min.css"
    };

    //var i18n = {
    //    cwd: "src/i18n/",
    //    dist: "dist/i18n/"
    //};

    var configConstants = {
        ngTemplatesHtmlMin: {
            collapseBooleanAttributes: true,
            collapseWhitespace: true,
            removeAttributeQuotes: true,
            removeComments: true,
            removeEmptyAttributes: true,
            removeRedundantAttributes: false,
            removeScriptTypeAttributes: true,
            removeStyleLinkTypeAttributes: true
        }
    };

    // Project configuration.
    grunt.initConfig({
        // pkg: grunt.file.readJSON("package.json"),

        jshint: {
            all: ["gruntfile.js", job.cwdJs]
        },

        clean: {
            tmp: tmpRoot + "**/*", 
            dist: "dist/**/*"
        },

        copy: {
            build: {
                files: [
                    {
                        expand: true,
                        cwd: job.cwd,
                        src: ["**", "!**/*spec.js", "!**/tests/**"],
                        dest: job.tmp
                    },
               
                    {
                        expand: true,
                        src: job.lib, 
                        dest: job.tmp
                    }
                ]
            },
            toMin: {
                src: job.concatFile,
                dest: job.uglifyFile
            }
        },
//        ngtemplates: {
//            editExt: {
//                options: {
//                    module: "tempCustomGpsTmplates",//"eavCustomFields",
////                    standalone: false,
//                    append: true,
//                    htmlmin: configConstants.ngTemplatesHtmlMin
//                },
//                files: [
//                     {
//                         cwd: editExt.tmp,
//                         src: ["**/*.html"], 
//                         dest: editExt.templates
//                     }
//                ]
//        },
        concat: {
            editExt: {
                src: job.tmp + "**/*.js",
                dest: job.concatFile
            }
        },
        ngAnnotate: {
            editExt: {
                expand: true,
                src: job.concatFile,
                extDot: "last"          // Extensions in filenames begin after the last dot 
            }
        },


        uglify: {
            options: {
                // banner: "/*! <%= pkg.name %> <%= grunt.template.today(\"yyyy-mm-dd hh:MM\") %> */\n",
                sourceMap: true
            },

            editExt: {
                src: job.concatFile,
                dest: job.uglifyFile
            }
        },

        watch: {
            ngUi: {
                files: [job.cwd + "**"],
                tasks: ["buildGps"]
            }
        }
    });

    // Load all grunt-plugins mentioned in the package.json
    require("load-grunt-tasks")(grunt);
    require("time-grunt")(grunt);


    // Default task.
    grunt.registerTask("buildGps", [
        "jshint",
        "clean:tmp",
        "copy",
        // "ngtemplates",
        "concat",
        "ngAnnotate",
        //,
        //"uglify"
        //,
        "watch:ngUi"
    ]);
     

};
module.exports = function(grunt) {
    "use strict";
    var distRoot = "dist/";
    var tmpRoot = "tmp/";
    var admin = {
        cwd: "src/admin/",
        cwdJs: ["src/admin/**/*.js"],
        tmp: "tmp/admin/",
        templates: "tmp/admin/html-templates.js",
        dist: "dist/admin/",
        concatFile: "dist/admin/eav-admin.js",
        uglifyFile: "dist/admin/eav-admin.min.js",
        concatCss: "dist/admin/eav-admin.css",
        concatCssMin: "dist/admin/eav-admin.min.css"
    };

    var editUi = {
        cwd: "src/edit/",
        cwdJs: "src/edit/**/*.js",
        tmp: "tmp/edit/",
        templates: "tmp/edit/html-templates.js",
        dist: "dist/edit/",
        concatFile: "dist/edit/eav-edit.js",
        uglifyFile: "dist/edit/eav-edit.min.js",
        concatCss: "dist/edit/edit.css",
        concatCssMin: "dist/edit/edit.min.css"
    };
    var i18n = {
        cwd: "src/i18n/",
        dist: "dist/i18n/"
    };

    var concatPipelineCss = "pipeline-designer.css";

    var js = {
        eav: {
            "src": "eav/js/src/**/*.js",
            "specs": "eav/js/specs/**/*spec.js",
            "helpers": "eav/js/specs/helpers/*.js"
        }
    };

    // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON("package.json"),

        jshint: {
            //ngUi: [admin.cwdJs],
            //edit: [editUi.cwdJs],
            all: ["gruntfile.js", admin.cwdJs, editUi.cwdJs]//, js.eav.specs, js.eav.src]
        },

        clean: {
            tmp: tmpRoot + "**/*", // '.tmp/**/*',
            dist: "dist/**/*"
        },

        copy: {
            build: {
                files: [
                    {
                        expand: true,
                        cwd: admin.cwd,
                        src: ["**", "!**/*spec.js", "!**/tests/**"],
                        dest: admin.tmp
                    },
                    {
                        expand: true,
                        cwd: editUi.cwd,
                        src: ["**", "!**/*spec.js", "!**/tests/**"],
                        dest: editUi.tmp
                    }
                ]
            },
            i18n: { 
                files: [
                    {
                        expand: true,
                        cwd: "src/i18n/", //i18n.cwd,
                        src: ["**/*.json"],
                        dest: "dist/i18n/", //  i18n.dist
                        rename: function(dest, src) {
                            return dest + src.replace(".json",".js");
                        }
                    }

                ]
            }
        },
        ngtemplates: {
            default: {
                options: {
                    module: "eavTemplates",
                    append: true,
                    htmlmin: {
                        collapseBooleanAttributes: true,
                        collapseWhitespace: true,
                        removeAttributeQuotes: true,
                        removeComments: true,
                        removeEmptyAttributes: true,
                        removeRedundantAttributes: false,
                        removeScriptTypeAttributes: true,
                        removeStyleLinkTypeAttributes: true
                    }
                },
                files: [
                    {
                        cwd: admin.tmp,// + "/",
                        src: ["**/*.html"], //, 'wrappers/**/*.html', 'other/**/*.html'],
                        dest: admin.templates
                    }
                ]
            },
            editUi: {
                options: {
                    module: "eavEditTemplates",
                    append: true,
                    htmlmin: {
                        collapseBooleanAttributes: true,
                        collapseWhitespace: true,
                        removeAttributeQuotes: true,
                        removeComments: true,
                        removeEmptyAttributes: true,
                        removeRedundantAttributes: false,
                        removeScriptTypeAttributes: true,
                        removeStyleLinkTypeAttributes: true
                    }
                },
                files: [
                     {
                         cwd: editUi.tmp,// + "/",
                         src: ["**/*.html"], //, 'wrappers/**/*.html', 'other/**/*.html'],
                         dest: editUi.templates
                     }
                ]
            }
        },
        concat: {
            default: {
                src: admin.tmp + "**/*.js",
                dest: admin.concatFile
            },
            adminCss: {
                src: admin.tmp + "**.css",
                dest: admin.concatCss
            },
            editUi: {
                src: editUi.tmp + "**/*.js",
                dest: editUi.concatFile
            },
            pipelineCss: {
                src: [admin.tmp + "pipelines/pipeline-designer.css"],
                dest: admin.dist + concatPipelineCss
            },
            editUiCss: {
                src: [editUi.tmp + "**/*.css"],
                dest: editUi.concatCss
            }

        },
        ngAnnotate: {
            default: {
                expand: true,
                src: admin.concatFile,
                extDot: "last"          // Extensions in filenames begin after the last dot 
            },
            editUi: {
                expand: true,
                src: editUi.concatFile,
                extDot: "last"          // Extensions in filenames begin after the last dot 
            }

        },


        uglify: {
            options: {
                banner: "/*! <%= pkg.name %> <%= grunt.template.today(\"yyyy-mm-dd hh:MM\") %> */\n",
                sourceMap: true
            },

            default: {
                src: admin.concatFile,
                dest: admin.uglifyFile
            },
            editUi: {
                src: editUi.concatFile,
                dest: editUi.uglifyFile
            }
        },
        
        cssmin: {
            options: {
                shorthandCompacting: false,
                roundingPrecision: -1
            },
            target: {
                files: [{
                    expand: true,
                    cwd: distRoot,
                    src: ["**/*.css", "!**/*.min.css"],
                    dest: distRoot,
                    ext: ".min.css"
                }
                ]
            }
        },

        compress: {
            main: {
                options: {
                    mode: "gzip"
                },
                expand: true,
                cwd: distRoot,
                src: ["**/*.min.js"],
                dest: distRoot,
                ext: ".gz.js"
            }
        },

        jasmine: {
            default: {
                // Your project's source files
                src: js.eav.src,
                options: {
                    // Your Jasmine spec files 
                    specs: js.eav.specs,
                    // Your spec helper files
                    helpers: js.eav.helpers
                }
            }
        },

        watch: {
            ngUi: {
                files: ["gruntfile.js", admin.cwd + "**", editUi.cwd + "**"],
                tasks: ["build"]
            },
            devEavMlJson: {
                files: ["gruntfile.js", js.eav.src, js.eav.specs],
                tasks: ["jasmine:default", "jasmine:default:build"]                
            }
        }
    });

    function loadConfig(path) {
        var glob = require('glob');
        var object = {};
        var key;

        glob.sync('*', { cwd: path }).forEach(function (option) {
            key = option.replace(/\.js$/, '');
            object[key] = require(path + option);
        });

        return object;
    }

    // Load all grunt-plugins mentioned in the package.json
    require("load-grunt-tasks")(grunt);


    // Default task.
    grunt.registerTask("build", [
        "jshint",
        "clean:tmp",
        "copy",
        "ngtemplates",
        "concat",
        "ngAnnotate",
        "uglify",
        "cssmin",
        //"clean:tmp",
        "watch:ngUi"
    ]);
    grunt.registerTask("lint", "jshint");
    grunt.registerTask("default", "jasmine");
    grunt.registerTask("manualDebug", "jasmine:default:build");
    grunt.registerTask("quickDebug", "quickly log a test", function() {
        grunt.log(admin.cwdJs);
    });
     
    // External task - read http://www.thomasboyt.com/2013/09/01/maintainable-grunt.html
    // note that for a reason I don't understand, ...
    // the options in each file override the other ones
    // so I have to uncomment only the file I want to execute
    // since library-preparation is not a common action, this is sufficient for now
    //grunt.task.loadTasks("grunt-tasks/angular-set");
    //grunt.task.loadTasks("grunt-tasks/jsplumb-jquery");
    //grunt.task.loadTasks("grunt-tasks/jsplumb-clean");
    //grunt.task.loadTasks("grunt-tasks/i18n");

};
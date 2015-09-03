// "use strict";
module.exports = function (grunt) {
    var cwd = 'eav/ng';                             // the source application root
    var tmp = 'tmp/ng/';                            // target for processing
    var preBuiltDest = tmp + 'build-prep';          // pre-processing folder
    var builtDest = tmp + 'built';                  // final folder
    var templatesFile = preBuiltDest + '/html-templates.js';
    var targetFilename = 'tosic-eav-admin';
    var concatFile = builtDest + '/' + targetFilename + '.js';
    var annotated = builtDest + '/' + targetFilename + '.annotated.js';
    var uglifyFile = builtDest + '/' + targetFilename + '.min.js';


    var js = {
        eav: { 
            "src": "eav/js/src/**/*.js",
            "specs": "eav/js/specs/**/*spec.js",
            "helpers": "eav/js/specs/helpers/*.js"
        },
        ngadmin: {
            allJs: ["eav/ng/**/*.js", "!**/*.annotated.js", "!eav/ng/dist/**"],
            "annotated": "eav/ng/**/*.annotated.js"
        }
    };

  // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        jshint: {
            all: ["gruntfile.js", js.eav.src, js.eav.specs, js.ngadmin.allJs]
        },

        clean: {
            tmp: tmp + '**/*', // '.tmp/**/*',
            dist: 'dist/**/*'
        },

        copy: {
            build: {
                files: [
                    {
                        expand: true,
                        cwd: cwd,
                        src: ['**', '!**/*Spec.js'],
                        dest: preBuiltDest
                    }
                ]
            }
            //,
            //dist: {
            //    expand: true,
            //    cwd: '.tmp/',
            //    src: '**/built/**/*.*',
            //    dest: 'dist/',
            //    flatten: true,
            //    filter: 'isFile'
            //}
        },
        ngtemplates: {
            default: {
                options: {
                    module: 'eavTemplates',
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
                        cwd: preBuiltDest + '/',
                        src: ['**/*.html'],//, 'wrappers/**/*.html', 'other/**/*.html'],
                        dest: templatesFile
                    }
                ]
            }
        },
        concat: {
            default: {
                src: preBuiltDest + '/**/*.js',
                dest: concatFile
            }

        },
        ngAnnotate: {
            default: {
                expand: true,
                src: concatFile,
                // dest: annotated,
                ext: '.annotated.js', // Dest filepaths will have this extension. 
                extDot: 'last' // Extensions in filenames begin after the last dot 
            }
        },


        uglify: {
            options: {
                banner: '/*! <%= pkg.name %> <%= grunt.template.today("yyyy-mm-dd hh:MM") %> */\n',
                sourceMap: true
            },

            default: {
                src: annotated,
                dest: uglifyFile
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
            files: ["gruntfile.js", js.eav.src, js.eav.specs],
            tasks: ['jasmine:default', 'jasmine:default:build']
        }
    });

  // Load the plugin that provides the "uglify" task.
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-jshint');
    grunt.loadNpmTasks('grunt-contrib-jasmine');
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-ng-annotate');
    grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-ng-templates');

    // Default task.
    grunt.registerTask('build', [
        'jshint',
        'clean:tmp',
        'copy',
        'ngtemplates',
        'concat',
        'ngAnnotate',
        'uglify']);
    grunt.registerTask('lint', 'jshint');
    grunt.registerTask('default', 'jasmine');
    grunt.registerTask('manualDebug', 'jasmine:default:build');
};
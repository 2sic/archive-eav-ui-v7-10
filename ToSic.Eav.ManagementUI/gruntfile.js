// "use strict";
module.exports = function (grunt) {
    js = {
        eav: { 
            "src": "eav/js/src/**/*.js",
            "specs": "eav/js/specs/**/*spec.js",
            "helpers": "eav/js/specs/helpers/*.js"
        },
        ngadmin: {
            "js": "eav/ng/**/*.js"
        }
    };

  // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        jshint: {
            all: ["gruntfile.js", js.eav.src, js.eav.specs, js.ngadmin.js]
        },
        
        ngAnnotate: {
            ngAdmin: {
                expand: true,
                src: js.ngadmin.js,
                ext: '.annotated.js', // Dest filepaths will have this extension. 
                extDot: 'last',       // Extensions in filenames begin after the last dot 
            }
        }


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

  //grunt.loadNpmTasks('grunt-ng-annotate');
  
  // Load the plugin that provides the "uglify" task.
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-jshint');
    grunt.loadNpmTasks('grunt-contrib-jasmine');
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-ng-annotate');

    // Default task.
    grunt.registerTask('lint', 'jshint');
    grunt.registerTask('default', 'jasmine');
    grunt.registerTask('manualDebug', 'jasmine:default:build');

};
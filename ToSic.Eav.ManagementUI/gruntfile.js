module.exports = function(grunt) {

  // Project configuration.
    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        jasmine: {
            default: {
                // Your project's source files
                src: "<%= pkg.scripts.eav.src %>",
                options: {
                    // Your Jasmine spec files
                    specs: "<%= pkg.scripts.eav.specs %>",
                    // Your spec helper files
                    helpers: 'eav/js/specs/helpers/*.js'
                }
            }
        },
        watch: { 
            files: ["gruntfile.js", "<%= pkg.scripts.eav.src %>", "<%= pkg.scripts.eav.specs %>"],
            tasks: ['jasmine']
        }

    });

  //grunt.loadNpmTasks('grunt-ng-annotate');
  
  // Load the plugin that provides the "uglify" task.
  //grunt.loadNpmTasks('grunt-contrib-uglify');

  //grunt.loadNpmTasks('grunt-contrib-jshint');

    // Register tasks.
    grunt.loadNpmTasks('grunt-contrib-jasmine');
    grunt.loadNpmTasks('grunt-contrib-watch');


    // Default task.
    grunt.registerTask('default', 'jasmine');

};
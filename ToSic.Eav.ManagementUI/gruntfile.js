module.exports = function(grunt) {

  // Project configuration.
    grunt.initConfig({
        // pkg: grunt.file.readJSON('package.json'),

        jasmine: {
            // Your project's source files
            src: 'eav/js/src/**/*.js',
            // Your Jasmine spec files
            specs: 'eav/js/specs/**/*spec.js',
            // Your spec helper files
            helpers: 'eav/js/specs/helpers/*.js'
        }

    });

  //grunt.loadNpmTasks('grunt-ng-annotate');
  
  // Load the plugin that provides the "uglify" task.
  //grunt.loadNpmTasks('grunt-contrib-uglify');

  //grunt.loadNpmTasks('grunt-contrib-jshint');

    // Register tasks.
    grunt.loadNpmTasks('grunt-contrib-jasmine');


    // Default task.
    grunt.registerTask('default', 'jasmine');

};
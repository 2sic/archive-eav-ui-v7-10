/* 
 * ANGULAR Translate SET !
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
    var i18n = {
    	banner: "/*! Angular-Translate Package for 2sic 2sxc & eav <%= grunt.template.today(\"yyyy-mm-dd hh:MM\") %> */\n",
		cwd: "bower_components/",
		jsFiles: [
			// i18n files
			"bower_components/angular-translate/angular-translate.min.js",
			"bower_components/angular-translate-loader-partial/angular-translate-loader-partial.min.js",
		],
        tmp: "tmp/lib/i18n/",
        dist: "dist/lib/i18n/",
        concatFile: "dist/lib/i18n/set.min.js",
        uglifyFile: "dist/lib/i18n/set.min.js",
        concatCss: "dist/lib/angular/set.min.css",
    };

    // Project configuration.
    grunt.config.merge({
    	clean: {
    		i18ntmp: i18n.tmp + "**/*", 
    		i18ndist: i18n.dist + "/*"
    	},
        concat: {
        	i18nimport: {
        		nonull: true,
                src: i18n.jsFiles,
                dest: i18n.concatFile
        	}
        },

    });

    // Default task.
    grunt.registerTask("build-i18n-lib-standalone", [
        "clean:i18ntmp",
        "clean:i18ndist",
        "concat:i18nimport"
    ]);
};
/* 
 * JS Plumb without jQuery !
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
    var pipelineDesigner = {
    	banner: "/*! jsPlumb-Set without jQuery for 2sic 2sxc & eav <%= grunt.template.today(\"yyyy-mm-dd hh:MM\") %> */\n",
		cwd: "bower_components/",
		jsFiles: [
            "bower_components/jsplumb/dist/js/jsPlumb-2.1.7.js"//"-min.js"
        ],
        concatFile: "dist/lib/pipeline/set.min.js",
    };

    // Project configuration.
    grunt.config.merge({
        concat: {
        	pdall: {
        		nonull: true,
                src: pipelineDesigner.jsFiles,
                dest: pipelineDesigner.concatFile
            }
        }
    });

    // Default task.
    grunt.registerTask("build-pd-clean-lib", [
        "concat:pdall"
    ]);
};
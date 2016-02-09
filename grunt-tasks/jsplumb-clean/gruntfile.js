/* 
 * JS Plumb and jQuery !
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
            "bower_components/jsplumb/dist/js/dom.jsPlumb-1.7.2-min.js",
            //"bower_components/jsplumb/dist/js/dom.jsPlumb-1.7.2.js",
        ],
        tmp: "tmp/lib/pipeline/",
        dist: "dist/lib/pipeline",
        concatFile: "dist/lib/pipeline/set.min.js",
        uglifyFile: "dist/lib/pipeline/set.min.js"
    };

    // Project configuration.
    grunt.config.merge({
    	clean: {
    		pdtmp: pipelineDesigner.tmp + "**/*", 
    		pddist: pipelineDesigner.dist + "/*"
    	},

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
        "clean:pdtmp", ,
        "clean:pddist",
        "concat:pdall"
    ]);
};
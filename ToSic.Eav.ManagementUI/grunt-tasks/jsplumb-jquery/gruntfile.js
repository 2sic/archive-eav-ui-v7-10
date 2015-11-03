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
    	banner: "/*! jQuery and jsPlumb-Set for 2sic 2sxc & eav <%= grunt.template.today(\"yyyy-mm-dd hh:MM\") %> */\n",
    	cwd: "bower_components/",
    	jqFiles: [
            "bower_components/jquery/dist/jquery.min.js",
            "bower_components/jquery-ui/jquery-ui.min.js",
    	],
		jsFiles: [
			// basic jquery
            //"bower_components/jquery/dist/jquery.min.js",
            //"bower_components/jquery-ui/jquery-ui.min.js",

            "bower_components/jsplumb/dist/js/jquery.jsPlumb-1.7.2-min.js",
        ],
        tmp: "tmp/lib/pipeline/",
        dist: "dist/lib/pipeline",
        jqConcat: "dist/lib/pipeline/jq.min.js",
        concatFile: "dist/lib/pipeline/set.min.js",
        uglifyFile: "dist/lib/pipeline/set.min.js"
    };

    // Project configuration.
    grunt.initConfig({
    	clean: {
    		tmp: pipelineDesigner.tmp + "**/*", 
    		dist: pipelineDesigner.dist + "/*"
    	},

        concat: {
        	pipelineDesigner: {
        		nonull: true,
                src: pipelineDesigner.jsFiles,
                dest: pipelineDesigner.concatFile
            },
        	jQuery: {
        		nonull: true,
                src: pipelineDesigner.jqFiles,
                dest: pipelineDesigner.jqConcat
        	},
        },

    });

    // Default task.
    grunt.registerTask("buildLibrary", [
        "clean",
        "concat",
        //"uglify",
		//"compress"
    ]);
};
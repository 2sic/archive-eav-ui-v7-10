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
		jsFiles: [
			// basic jquery
            "bower_components/jquery/dist/jquery.min.js",
            "bower_components/jquery-ui/jquery-ui.min.js",

            "bower_components/jsplumb/dist/js/jquery.jsPlumb-1.7.2-min.js",
        ],
        tmp: "tmp/lib/pipeline/",
        dist: "dist/lib/pipeline",
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
            }
        },


        uglify: {
            options: {
                banner: pipelineDesigner.banner,
                sourceMap: false
            },

            pipelineDesigner: {
                src: pipelineDesigner.concatFile,
                dest: pipelineDesigner.uglifyFile
            }
        },

		// compress not used for now, because the iis seems to re-compress it causing strange side-effects
        compress: {
            main: {
                options: {
                    mode: "gzip"
                },
                expand: true,
                cwd: pipelineDesigner.dist,
                src: ["**/*.min.js"],
                dest: pipelineDesigner.dist,
                ext: ".gz.js"
            }
        }
    });

    // Default task.
    grunt.registerTask("buildPipelineDesignerLib", [
        "clean",
        "concat",
        //"uglify",
		//"compress"
    ]);
};
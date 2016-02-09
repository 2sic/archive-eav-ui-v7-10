/* 
 * AG-Grid (Agnostic Grid) --> http://www.ag-grid.com/
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
    var aggrid = {
    	//banner: "/*! AG Grid for 2sic eav <%= grunt.template.today(\"yyyy-mm-dd hh:MM\") %> */\n",
    	//cwd: "<%= paths.bower %>",
		jsFiles: [
            "<%= paths.bower %>/ag-grid/dist/ag-grid.min.js",
		],
		cssFiles: [
            "<%= paths.bower %>/ag-grid/dist/ag-grid.min.css",
		],
        //tmp: "<%= paths.tmp %>/lib/ag-grid/",
        //dist: "<%= paths.tmp %>/lib/ag-grid",
		concatFile: "<%= paths.dist %>/lib/ag-grid/ag-grid.min.js",
		concatCss: "<%= paths.dist %>/lib/ag-grid/ag-grid.min.css",
        uglifyFile: "<%= paths.dist %>/lib/ag-grid/ag-grid.min.js"
    };

    // Project configuration.
    grunt.config.merge({
    	clean: {
    		aggridtmp: aggrid.tmp + "**/*", 
    		aggriddist: aggrid.dist + "/*"
    	},

        concat: {
            aggrid: {
        		nonull: true,
                src: aggrid.jsFiles,
                dest: aggrid.concatFile
            },
            aggridCss: {
        		nonull: true,
                src: aggrid.cssFiles,
                dest: aggrid.concatCss
            }
        }
    });

    // Default task.
    grunt.registerTask("build-lib-aggrid", [
        //"clean:aggridtmp", 
        //"clean:aggriddist",
        "concat:aggrid",
        "concat:aggridCss"
    ]);
};
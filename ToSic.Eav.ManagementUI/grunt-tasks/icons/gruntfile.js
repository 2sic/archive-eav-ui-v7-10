
// contains all the grunt config for the in-page fonts

module.exports = function (grunt) {

    grunt.config.merge({
        fonts: {
            cwd: "<%= paths.bower %>/2sxc-icons/",
            dist: "<%= paths.dist %>/lib/fonts/",
            dev: "<%= paths.src %>/admin/",
            inpage: "<%= paths.src %>/inpage/"
        },

        copy: {
            fonts: {
                files: [
                    {
                        note: "font files",
                        expand: true,
                        flatten: true,
                        cwd: "<%= fonts.cwd %>",
                        src: ["**/app-icons.woff"],
                        dest: "<%= fonts.dist %>"
                    },
                    {
                        note: "full system css definition for current characters",
                        expand: true,
                        flatten: true,
                        cwd: "<%= fonts.cwd %>",
                        src: ["full-system/css/app-icons-codes.css"],
                        dest: "<%= fonts.dev %>"
                    }

                ]
            }
        }

    });

    grunt.registerTask("import-icons", ["copy:fonts"]);

};
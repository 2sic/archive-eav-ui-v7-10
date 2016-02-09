/// <binding ProjectOpened='build-auto' />
module.exports = function (grunt) {
    "use strict";
    var srcRoot = "src/i18n/",
        srcRootEn = "src/en-master/",
        distRoot = "dist/",
        tmpRoot = "tmp/",
        tmpAnalytics = "tmp/analyt/",
        analysisRoot = "analysis/",
        packs = [
            "admin",    // for the EAV admin UI
            "edit",     // for the EAV edit UI
            "inpage",   // for the 2sxc in-page button / dialogs
            "sxc-admin" // for the 2sxc admin UIs like App, Manage Apps, etc.
        ],
        languages = [
            "en",   // English, core distribution, responsible: 2sic
            "de",   // German - responsible 2sic
            "es",   // Spanish - responsible ...
            "fr",   // French - responsible BSI
            "it",   // Italian - responsible Opsi
            "uk"    // Ukranian - responsible ForDnn
        ];

    var mergeFiles = {};

    packs.forEach(function(pack) {
        languages.forEach(function (lang) {
            mergeFiles[distRoot + "i18n/" + pack + "-" + lang + ".js"]
                = [((lang === "en") ? srcRootEn : srcRoot) + "**/" + pack + "-" + lang + ".json"];
        });
    });

    grunt.initConfig({
        pkg: grunt.file.readJSON("package.json"),

        clean: {
            dist: distRoot + "**/*", // only do this when you will re-copy the eav stuff into here
            tmp: tmpRoot + "**/*"
        },

        /* This is an experimental block - will merge the few changes in -en-uk with the master -en */
        /* 2015-11-29 dm - seems to work, but not very generic yet. Will persue more when I have a real use case */
        //"merge-json": {
        //    "en-uk": {
        //        src: [ srcRoot + "admin-en.json", srcRoot + "admin-en-uk.json" ],
        //        dest: tmpRoot + "admin-en-uk.js"
        //    }
        //},
        
        copy: {
            enUk: {
                files: [

                ]
            },
            //i18n: {
            //    files: [
            //        {
            //            expand: true,
            //            cwd: "src/i18n/", 
            //            src: ["**/*.json"],
            //            dest: "dist/i18n/", 
            //            rename: function (dest, src) {
            //                return dest + src.replace(".json", ".js");
            //            }
            //        }
            //    ]
            //},
            "import-tinymce-libs": {
                files: [
                    {
                        expand: true,
                        flatten: false,
                        cwd: "src/i18n-lib/", 
                        src: ["**/*.js"],
                        dest: "dist/i18n/lib/"
                    }
                ]
            }
        },
        "merge-json": {
            "all": {
                files: mergeFiles
                //    {
                //    "tmp/i18n/edit-de.js": [srcRoot + "**/edit-de.json"],
                //    //"www/de.json": [ "src/**/*-de.json" ]
                //}
            }
        },
        /* Experiment to flatten the JSON for reviewing translation completeness */
        //flatten_json: {
        //    main: {
        //        files: [
        //            {
        //                expand: true,
        //                src: [srcRoot + "*-en.json"],
        //                dest: analysisRoot
        //            }
        //        ]
        //    }
        //},

        /* Experimeting with wrapping the flattened jsons with [ and ] */
        /* note: doesn't work ATM*/
        //concat: {
        //    options: {
        //        banner: "[",
        //        footer: "]"
        //    },
        //    default: {
        //        src: [analysisRoot + "**/*.json"],
        //        dest: tmpAnalytics
        //        //,
        //        //dest: 
        //    }
        //    //default_options: {
        //    //    files: [
        //    //      {
        //    //          prepend: "[",
        //    //          append: "]",
        //    //          input: analysisRoot + "**/*.json" //,
        //    //          //output: 'path/to/output/file'
        //    //        }
        //    //     ]
        //    //}
        //},

        /* Watchers to auto-compile while working on it */
        watch: {
            options: {
                atBegin: true
            },
            i18n: {
                files: ["gruntfile.js", "src/**"],
                tasks: ["build"]
            }
        }


    });

    grunt.loadNpmTasks("grunt-contrib-watch");
    grunt.loadNpmTasks("grunt-contrib-copy");
    grunt.loadNpmTasks("grunt-contrib-clean");
    grunt.loadNpmTasks("grunt-merge-json");
    // grunt.loadNpmTasks("grunt-flatten-json");
    // grunt.loadNpmTasks("grunt-contrib-concat");

    // Default task(s).
    grunt.registerTask("build-auto", ["watch:i18n"]);
    grunt.registerTask("build", [
        //"clean:tmp",
        "merge-json"
    ]);
};
// Initial variables, constants, etc.
var gulp = require("gulp"),
    $ = require("gulp-load-plugins")({ lazy: false }),
    packageJSON = require('./package'),
    // would need this to always auto-publish after compile... runSequence = require('run-sequence'),
    jshintConfig = packageJSON.jshintConfig,
    merge = require("merge-stream"),
    js = "js",
    css = "css",
    config = {
        debug: true,
        autostart: true,
        autopublish: true,
        autopublishTarget: "./../2SexyContent/Web/DesktopModules/ToSIC_SexyContent/dist",
        rootDist: "dist/" // "tmp-gulp/dist/"
    };

// setup admin, exclude pipeline css (later also exclude pipeline js)
var admin = createConfig("admin", "eavTemplates");
admin.css.files.push("!" + admin.cwd + "**/pipeline*.css");

// setup edit & extended
var edit = createConfig("edit", "eavEditTemplates");

// pipeline-designer (CSS only)
var pDesigner = createConfig("admin", "");
pDesigner.css.files = [admin.cwd + "**/pipeline*.css"];
pDesigner.css.concat = "pipeline-designer.css";

// extension: gps-field
var editExtGps = createConfig("edit-extended");
editExtGps.cwd = editExtGps.cwd.replace("/edit-extended/", "/edit-extended/fields/custom-gps/");
editExtGps.dist = editExtGps.dist.replace("/edit-extended/", "/edit/extensions/field-custom-gps/");
editExtGps.js.concat = "custom-gps.js";
editExtGps.js.libs = [
    "bower_components/lodash/dist/lodash.min.js",
    "bower_components/angular-google-maps/dist/angular-google-maps.min.js",
    "bower_components/angular-simple-logger/dist/index.js"
];
editExtGps.js.autoSort = false;

// part: i18n library
var i18n = createConfig("i18n", undefined, config.rootDist + "lib/i18n/", "set.js", [
	"bower_components/angular-translate/angular-translate.min.js",
	"bower_components/angular-translate-loader-partial/angular-translate-loader-partial.min.js",
]);
i18n.js.autoSort = false;
i18n.js.uglify = false;

// part: ag-grid library
var agGrid = createConfig("ag-grid", undefined, config.rootDist + "lib/ag-grid/", "ag-grid.js", [
    "bower_components/ag-grid/dist/ag-grid.min.js",
]);
agGrid.css.files = ["bower_components/ag-grid/dist/ag-grid.min.css"];
agGrid.js.uglify = false;


// part: jsPlumb
var jsPlumb = createConfig("jsPlumb", undefined, config.rootDist + "lib/pipeline/", "set.js", [
    "bower_components/jsplumb/dist/js/jsPlumb-2.1.7.js"
]);
jsPlumb.js.uglify = false;

// lib Angular
var libAng = createConfig("angular", undefined, config.rootDist + "lib/angular/", "set.js", [
			// the basic files
            "bower_components/angular/angular.min.js",
			"bower_components/angular-resource/angular-resource.min.js",
            "bower_components/angular-animate/angular-animate.min.js",
            "bower_components/angular-sanitize/angular-sanitize.min.js", // currently testing, needed for ui-select, maybe will remove again
            "bower_components/oclazyload/dist/oclazyload.min.js",

			// visual effects etc.
            "bower_components/angular-bootstrap/ui-bootstrap-tpls.min.js",
            "bower_components/angular-toastr/dist/angular-toastr.tpls.min.js",

			// i18n files
			"bower_components/angular-translate/angular-translate.min.js",
			"bower_components/angular-translate-loader-partial/angular-translate-loader-partial.min.js",

			// files used by formly and the general edit UI
			"bower_components/api-check/dist/api-check.min.js",
			"bower_components/angular-base64-upload/dist/angular-base64-upload.min.js",
			"bower_components/angular-formly/dist/formly.min.js",
			"bower_components/angular-formly-templates-bootstrap/dist/angular-formly-templates-bootstrap.min.js",
            "bower_components/angular-ui-tree/dist/angular-ui-tree.min.js",

            // testing
            "bower_components/angular-ui-select/dist/select.min.js",

            // promise-window just to be sure we can use it till all old dialogs have been removed
			// "bower_components/promise-window/dist/promise-window.min.js",

            // dropzone for uploads
            "bower_components/dropzone/dist/min/dropzone.min.js",

            // switch for toggle-feature
            "bower_components/angular-ui-switch/angular-ui-switch.min.js",
]);
libAng.css.files = [
            "bower_components/bootstrap/dist/css/bootstrap.min.css",
            "bower_components/bootflat-for-2sic/bootflat/css/bootflat.min.css",
            "bower_components/angular-ui-tree/dist/angular-ui-tree.min.css",
            "bower_components/angular-ui-switch/angular-ui-switch.min.css",

            // toaster
            "bower_components/angular-toastr/dist/angular-toastr.css",

            // testing
            "bower_components/angular-ui-select/dist/select.min.css",
];
libAng.js.autoSort = false;
libAng.js.uglify = false;


gulp.task("test-libAng", function() {
    gulp.watch(libAng.cwd + "**/*", createWatchCallback(libAng, js));
});

// register all watches & run them
gulp.task("watch-all", function () {
    gulp.watch(admin.cwd + "**/*", createWatchCallback(admin, js));
    gulp.watch(admin.cwd + "**/*", createWatchCallback(admin, css));
    gulp.watch(edit.cwd + "**/*", createWatchCallback(edit, js));
    gulp.watch(edit.cwd + "**/*", createWatchCallback(edit, css));

    gulp.watch(pDesigner.files, createWatchCallback(pDesigner, css));

    gulp.watch(editExtGps.cwd + "**/*", createWatchCallback(editExtGps, js));
    gulp.watch(i18n.cwd + "**/*", createWatchCallback(i18n, js));
    gulp.watch(agGrid.cwd + "**/*", createWatchCallback(agGrid, js));
    //no css yet: gulp.watch(editExtGps.cwd + "**/*", createWatchCallback(editExtGps, css));
});

// test something - add your code here to test it
gulp.task("test-something", function () {

});

gulp.task("clean-dist", function () {
    // disabled 2016-03-13 to prevent mistakes as gulp doesn't generate everything yet
    //gulp.src(config.rootDist)
    //    .pipe($.clean());
});

// deploy the result to the current 2sxc-dev
gulp.task("publish-dist-to-2sxc", function () {
    gulp.src(["./dist/**/*", "!./dist/i18n/**/*", "!./dist/lib/fonts/**/*"])
    .pipe(gulp.dest(config.autopublishTarget));
});
gulp.task("watch-publish-dist-to-2sxc", function() {
    gulp.watch("dist/**/*", ['publish-dist-to-2sxc']);
});

//#region basic functions I'll need a lot
function createConfig(key, tmplSetName, altDistPath, altJsName, libFiles) {
    var cwd = "src/" + key + "/";
    return {
        name: key,
        cwd: cwd,
        dist: altDistPath || config.rootDist + key + "/",
        css: {
            files: [cwd + "**/*.css"],
            concat: "eav-" + key + ".css"
        },
        js: {
            files: [cwd + "**/*.js", "!" + cwd + "**/*spec.js", "!" + cwd + "**/tests/**"],
            libs: libFiles || [],
            concat: altJsName || "eav-" + key + ".js",
            templates: ["src/" + key + "/**/*.html"],
            templateSetName: tmplSetName,
            autoSort: true,
            uglify: true
        }
    }
}

// package a JS set
function packageJs(set) {
    if (config.debug) console.log("bundling start: " + set.name);

    var js = gulp.src(set.js.files);
    if (set.js.autoSort)
        js = js.pipe($.sort());
    js = js.pipe($.jshint(jshintConfig))
        .pipe($.jshint.reporter("jshint-stylish"))
        //.pipe($.jshint.reporter('fail'))
        .pipe($.ngAnnotate());

    var tmpl = set.js.templates ? gulp.src(set.js.templates)
        .pipe($.sort())
        //.pipe($.htmlmin({ collapseWhitespace: true }))
        .pipe($.angularTemplatecache("templates.js", { // set.js.templateSetName + ".js", { //"templates.js", {
            standalone: true,
            module: set.js.templateSetName // "eavTemplates"
        })) : null;

    var libs = gulp.src(set.js.libs);

    var prelib = merge(js, tmpl);
    if (set.js.autoSort)
        prelib = prelib.pipe($.sort());

    var result = merge(libs, prelib);
    if (set.js.autoSort)
        result = result.pipe($.sort());

    result = result.pipe($.concat(set.js.concat))
        .pipe(gulp.dest(set.dist))
        .pipe($.rename({ extname: ".min.js" }));
        // 2016-04-23 2dm had to disable source-maps for now, something is buggy inside
        // 2016-09-07 2dm re-enabled it, seems to work now...
    // 2016-09-08 2rm had to disable it again, sourcmap generator throws an error
    if (set.js.uglify)
        result = result
                //.pipe($.sourcemaps.init({ loadMaps: true }))
                .pipe($.uglify())
                .on("error", $.util.log)
            // .pipe($.sourcemaps.write("./"))
            ;
    result = result.pipe(gulp.dest(set.dist));

    if (config.debug) console.log($.util.colors.cyan("bundling done: " + set.name));

    return result;
}

// package a set of CSS
function packageCss(set) {
    if (config.debug) console.log("css packaging start: " + set.name);

    var result = gulp.src(set.css.files)
        .pipe($.sort())
        // lint the css - not enabled right now, too many fix-suggestions
        //.pipe($.csslint())
        //.pipe($.csslint.reporter())

        // concat & save concat-only (for debugging)
        .pipe($.concat(set.css.concat))
        .pipe(gulp.dest(set.dist))

        // minify and save
        .pipe($.rename({ extname: ".min.css" }))
        .pipe($.sourcemaps.init())
        .pipe($.cleanCss({ compatibility: "*", processImportFrom: ['!fonts.googleapis.com'] /* ie9 compatibility */ }))
        .pipe($.sourcemaps.write("./"))
        .pipe(gulp.dest(set.dist));
    ;
    if (config.debug) console.log($.util.colors.cyan("css packaging done: " + set.name));
    return result;
}

// assemble a function which will call the desired set - this is a helper for the watch-sequence. 
function createWatchCallback(set, part) {
    if (config.debug) console.log("creating watcher callback for " + set.name);
    var run = function (event) {
        if (config.debug) console.log("File " + event.path + " was " + event.type + ", running tasks on set " + set.name);
        var call = (part === js) ? packageJs : packageCss;
        call(set);
        console.log("finished '" + set.name + "'" + new Date());
        //if (config.autopublish) {
        //    console.log("publishing...");
        //    //running.pipe(
        //    runSequence(running, "publish-dist-to-2sxc");
        //    //gulp.start("publish-dist-to-2sxc");
        //    //);
        //    console.log("publishing done...");
        //}
    }
    if (config.autostart)
        run({ path: "[none]", type: "autostart" });
    return run;
}


//#endregion



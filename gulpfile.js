var gulp = require("gulp"),
    $ = require("gulp-load-plugins")({ lazy: false }),
    merge = require("merge-stream"),
    debug = true,
    js = "js",
    css = "css";

// General configuration
var config = {
    templateCache: {
        file: "templates.js",
        options: {
            module: "app.core",
            root: "app/",
            standAlone: false
        }
    },
    temp: "./.tmp/"
};

// setup admin, exclude pipeline css (later also exclude pipeline js)
var admin = createConfig("admin");
admin.css.files.push("!" + admin.cwd + "**/pipeline*.css");

// setup edit & extended
var edit = createConfig("edit");
var editExtGps = createConfig("edit-extended");
editExtGps.cwd = editExtGps.cwd.replace("/edit-extended/", "/edit-extended/fields/custom-gps/");
editExtGps.dist = editExtGps.dist.replace("/edit-extended/", "/edit/extensions/field-custom-gps/");
editExtGps.js.concat = "custom-gps.js";
editExtGps.js.libs = [
    "bower_components/lodash/lodash.min.js",
    "bower_components/angular-google-maps/dist/angular-google-maps.min.js",
    "bower_components/angular-simple-logger/dist/index.js"
];


//#region basic functions I'll need a lot
function createConfig(key) {
    var cwd = "src/" + key + "/";
    return {
        name: key,
        cwd: cwd,
        dist: "tmp-gulp/dist/" + key + "/",
        css: {
            files: [cwd + "**/*.css"], //, "!**/pipeline*.css"],
            concat: "eav-" + key + ".css"
        },
        js: {
            files: [cwd + "**/*.js", "!" + cwd + "**/*spec.js", "!" + cwd + "**/tests/**"],
            libs: [],
            concat: "eav-" + key + ".js",
            templates: ["src/" + key + "/**/*.html"]
        }
    }
}

function packageJs(set) {
    if (debug) console.log("bundling start: " + set.name);

    var js = gulp.src(set.js.files)
        .pipe($.jshint())
        .pipe($.ngAnnotate())
    ;

    var tmpl = gulp.src(set.js.templates)
        //.pipe($.htmlmin({ collapseWhitespace: true }))
        .pipe($.angularTemplatecache("templates.js", {
            module: "eavTemplates"
        }));

    var libs = gulp.src(set.js.libs);

    var result = merge(js, tmpl, libs)
        .pipe($.concat(set.js.concat))
        .pipe(gulp.dest(set.dist))
        .pipe($.rename({ extname: ".min.js" }))
        .pipe($.sourcemaps.init({ loadMaps: true }))
            .pipe($.uglify())
            .on("error", $.util.log)
        .pipe($.sourcemaps.write("./"))
        .pipe(gulp.dest(set.dist));

    if (debug) console.log("bundling done: " + set.name);

    return result;
}

function packageCss(set) {
    if (debug) console.log("css packaging start: " + set.name);

    var result = gulp.src(set.css.files)
        // lint the css - not enabled right now, too many fix-suggestions
        //.pipe($.csslint())
        //.pipe($.csslint.reporter())

        // concat & save concat-only (for debugging)
        .pipe($.concat(set.css.concat))
        .pipe(gulp.dest(set.dist))

        // minify and save
        .pipe($.rename({ extname: ".min.css" }))
        .pipe($.sourcemaps.init())
        .pipe($.cleanCss({ compatibility: "*" /* ie9 compatibility */ }))
        .pipe($.sourcemaps.write("./"))
        .pipe(gulp.dest(set.dist));
    ;
    if (debug) console.log("css packaging done: " + set.name);
    return result;
}
//#endregion

gulp.task("test-css", function() {
    packageCss(admin);
});

gulp.task("build-test-run", function () {
    packageJs(admin);
});

gulp.task('watch-all', function () {
    gulp.watch(admin.cwd + "**/*", createWatchCallback(admin, js));
    gulp.watch(admin.cwd + "**/*", createWatchCallback(admin, css));
    gulp.watch(edit.cwd + "**/*", createWatchCallback(edit, js));
    gulp.watch(edit.cwd + "**/*", createWatchCallback(edit, css));
    gulp.watch(editExtGps.cwd + "**/*", createWatchCallback(editExtGps, js));
    //no css yet: gulp.watch(editExtGps.cwd + "**/*", createWatchCallback(editExtGps, css));
});

// assemble a function which will call the desired set
function createWatchCallback(set, part) {
    if (debug) console.log('creating watcher callback for ' + set.name);
    //if (part === js)
        return function (event) {
            if (debug) console.log('File ' + event.path + ' was ' + event.type + ', running tasks on set ' + set.name);
            if(part === js)
                packageJs(set);
            if (part === css)
                packageCss(set);
            console.log("finished '" + set.name + "'");
        }
    //if (part === css)
    //    return function (event) {
    //        if (debug) console.log('File ' + event.path + ' was ' + event.type + ', running tasks on set ' + set.name);
    //        packageCss(set);
    //        console.log("finished '" + set.name + "'");
    //    }
}


// deploy the result to the current 2sxc-dev
gulp.task("publish-dist-to-2sxc", function () {
    gulp.src("./dist/**/*")// '*.{ttf,woff,eof,svg}')
    .pipe(gulp.dest("./../2SexyContent/Web/DesktopModules/ToSIC_SexyContent/dist"));
});


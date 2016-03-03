var gulp = require("gulp");
//var jshint = require('gulp-jshint');
var $ = require("gulp-load-plugins")({ lazy: false }),
    merge = require("merge-stream");

// Initial variables
//var admin = {
//    cwd: "src/admin/",
//    cwdJs: ["src/admin/**/*.js"],
//    tmp: "tmp/admin/",
//    templates: "tmp/admin/html-templates.js",
//    dist: "dist/admin/",
//    concatFile: "dist/admin/eav-admin.js",
//    uglifyFile: "dist/admin/eav-admin.min.js",
//    concatCss: "dist/admin/eav-admin.css",
//    concatCssMin: "dist/admin/eav-admin.min.css"
//};
function createConfig(key) {
    return {
        cwd: "src/" + key + "/",
        cwdJs: ["src/" + key + "/**/*.js"],
        // tmp: "tmp/" + key + "/",
        templates: "tmp/" + key + "/html-templates.js",
        dist: "tmp-gulp/dist/" + key + "/",
        concatFile: "eav-" + key + ".js",
        uglifyFile: "eav-" + key + ".min.js",
        concatCss: "dist/" + key + "/eav-" + key + ".css",
        concatCssMin: "dist/" + key + "/eav-" + key + ".min.css"
    }
}

var admin = createConfig("admin");

// General configuration
var config = {
    //htmltemplates: clientApp + '**/*.html',
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

gulp.task("build-beta", function () {
    var js = gulp.src(admin.cwdJs)
        .pipe($.jshint())
        .pipe($.ngAnnotate())
    ;

    var tmpl = gulp.src(admin.templates)
        //.pipe($.htmlmin({ collapseWhitespace: true }))
        .pipe($.angularTemplatecache("templates.js", {
            module: "eavTemplates"
        }));

    merge(js, tmpl)
        .pipe($.concat(admin.concatFile))
        .pipe(gulp.dest(admin.dist))
        .pipe($.rename({ extname: ".min.js" }))
        .pipe($.sourcemaps.init({ loadMaps: true }))
            .pipe($.uglify())
            .on("error", $.util.log)
        .pipe($.sourcemaps.write("./"))
        .pipe(gulp.dest(admin.dist));
});

// deploy the result to the current 2sxc-dev
gulp.task("copy-dist-to-2sxc", function () {
    gulp.src("./dist/**/*")// '*.{ttf,woff,eof,svg}')
    .pipe(gulp.dest("./../2SexyContent/Web/DesktopModules/ToSIC_SexyContent/dist"));
});


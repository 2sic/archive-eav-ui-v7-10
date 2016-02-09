(function () {

    angular.module("ContentFormlyTypes")

        .config(function (formlyConfigProvider) {
            var formly = formlyConfigProvider;

            formly.setType({
                name: "file",
                template: "<span class='btn btn-default btn-square btn-file'><span class='glyphicon glyphicon-open'></span><input type='file' ng-model='model[options.key]' base-sixty-four-input /></span> <span ng-if='model[options.key]'>{{model[options.key].filename}}</span>",
                wrapper: ["bootstrapLabel", "bootstrapHasError"]
            });

            formly.setType({
                name: "hidden",
                template: "<input style='display:none' ng-model='model[options.key]' />",
                wrapper: ["bootstrapLabel", "bootstrapHasError"]
            });
    });
}());
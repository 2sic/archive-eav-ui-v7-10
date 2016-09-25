(function () {
    /*jshint laxbreak:true */
    angular.module("ContentTypesApp")
        .controller("FieldsAdd", contentTypeFieldsAddController)
    ;

    /// This is the main controller for adding a field
    /// Add is a standalone dialog, showing 10 lines for new field names / types
    function contentTypeFieldsAddController(appId, svc, $filter, $uibModalInstance) {
        var vm = this;

        // prepare empty array of up to 10 new items to be added
        var nw = svc.newItem;
        vm.items = [nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw(), nw()];

        vm.item = svc.newItem();
        vm.types = svc.types.liveList();

        vm.allInputTypes = svc.getInputTypesList();
        //svc.getInputTypes().then(function (result) {
        //    function addToList(value, key) {
        //        var item = {
        //            dataType: value.Type.substring(0, value.Type.indexOf("-")),
        //            inputType: value.Type, 
        //            label: value.Label,
        //            description: value.Description
        //        };
        //        vm.allInputTypes.push(item);
        //    }

        //    angular.forEach(result.data, addToList);

        //    vm.allInputTypes = $filter("orderBy")(vm.allInputTypes, ["dataType", "inputType"]);
        //});

        vm.resetSubTypes = function resetSubTypes(item) {
            item.InputType = item.Type.toLowerCase() + "-default";
        };

        vm.ok = function () {
            var items = vm.items;
            var newList = [];
            for (var c = 0; c < items.length; c++)
                if (items[c].StaticName)
                    newList.push(items[c]);
            svc.addMany(newList, 0);
            $uibModalInstance.close();
        };

        vm.close = function() { $uibModalInstance.dismiss("cancel"); };
    }
}());
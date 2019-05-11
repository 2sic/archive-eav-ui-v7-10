<!DOCTYPE html>
<html style="height: 100%;">

    <head>
        <title>ag-Grid Angular Example</title>
        <meta name="description" content="AngularJS Grid Example">
        <meta name="keywords" content="angular angularjs grid table example"/>
        <meta name="viewport" content="width=device-width, initial-scale=1.0">

        <!-- Bootstrap -->
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/css/bootstrap.min.css">
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/css/bootstrap-theme.min.css">
        <link rel="stylesheet" href="./style.css">

        <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js"></script>
        <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js"></script>

        <link href="//maxcdn.bootstrapcdn.com/font-awesome/4.2.0/css/font-awesome.min.css" rel="stylesheet">

        <link rel="shortcut icon" href="http://www.ag-grid.com/favicon.ico" />

        <script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.3.8/angular.min.js"></script>

        <script src="./dist/ag-grid.js?ignore=notused14"></script>
        <link rel="stylesheet" type="text/css" href="./dist/ag-grid.css?ignore=notused14">
        <link rel="stylesheet" type="text/css" href="./dist/theme-fresh.css?ignore=notused14">
        <link rel="stylesheet" type="text/css" href="./dist/theme-dark.css?ignore=notused14">
        <link rel="stylesheet" type="text/css" href="./dist/theme-blue.css?ignore=notused14">

        <script src="example.js"></script>

        <style>
            label {
                font-weight: normal !important;
            }

            .blue {
                background-color: darkblue;
                color: lightblue;
            }
        </style>
    </head>

    <body ng-app="agGridApp" ng-controller="mainController" style="height: 100%; margin: 0px; padding: 0px;">

        <div style="position: absolute; top: 55px; left: 0px; padding: 0px 20px 20px 20px;">

            <nav class="navbar-inverse navbar-fixed-top">
                <div class="container">
                    <div class="row">
                        <div class="col-md-12 top-header big-text">
                        <span class="top-button-wrapper">
                            <a class="top-button" href="/"> <i class="fa fa-home"></i> Home</a>
                        </span>
                        <span class="top-button-wrapper">
                            <a class="top-button-selected" href="/example.php"> <i class="fa fa-bicycle"></i> Test Drive</a>
                        </span>
                        <span class="top-button-wrapper">
                            <a class="top-button" href="/documentation.php">  <i class="fa fa-book"></i> Documentation</a>
                        </span>
                        <span class="top-button-wrapper">
                            <a class="top-button" href="/media.php"> <i class="fa fa-road"></i> Media</a>
                        </span>
                        <span class="top-button-wrapper">
                            <a class="top-button" href="/forum"> <i class="fa fa-users"></i> Forum</a>
                        </span>
                        </div>
                    </div>
                </div>
            </nav>

            <!-- First row of header, has table options -->
            <div style="padding: 4px;">
                Rows:
                <select ng-model="rowCount" ng-change="onRowCountChanged()">
                    <option value="10">10</option>
                    <option value="100">100</option>
                    <option value="1000">1,000</option>
                    <option value="10000">10,000</option>
                    <option value="30000">30,000</option>
                    <option value="50000">50,000</option>
                    <option value="100000">100,000</option>
                </select>
                Cols:
                <select ng-model="colCount" ng-change="onColCountChanged()">
                    <option value="5">5</option>
                    <option value="10">10</option>
                    <option value="20">20</option>
                    <option value="50">50</option>
                    <option value="100">100</option>
                </select>

                Pinned:
                <select ng-model="pinnedColumnCount" ng-change="onPinnedColCountChanged()">
                    <option value="0">0</option>
                    <option value="1">1</option>
                    <option value="2">2</option>
                    <option value="3">3</option>
                    <option value="4">4</option>
                    <option value="5">5</option>
                </select>

                Size:
                <select ng-model="size" ng-change="onSize()">
                    <option value="fill">Fill Page</option>
                    <option value="fixed">800x600</option>
                </select>

                Group Headers:
                <select ng-model="groupHeaders" style="width: 60px;" ng-change="onGroupHeaders()">
                    <option value="true">Yes</option>
                    <option value="false">No</option>
                </select>

                Style:
                <select ng-model="style" style="width: 90px;">
                    <option value="">-none-</option>
                    <option value="ag-fresh">Fresh</option>
                    <option value="ag-blue">Blue</option>
                    <option value="ag-dark">Dark</option>
                </select>

                <span style="padding-left: 20px; display: inline-block;">
                    Jump to:
                    <input placeholder="row" type="text" ng-model="jumpToRowText" ng-change="jumpToRow()" style="width: 40px"/>
                    <input placeholder="col" type="text" ng-model="jumpToColText" ng-change="jumpToCol()" style="width: 40px"/>
                </span>
            </div>

            <div style="padding: 4px;">

                <input placeholder="Filter..." type="text" ng-model="gridOptions.quickFilterText"/>

                Selection:
                <select ng-model="rowSelection" ng-change="onSelectionChanged()" style="width: 100px;">
                    <option value="">-none-</option>
                    <option value="checkbox">Checkbox</option>
                    <option value="single">Single</option>
                    <option value="multiple">Multiple</option>
                </select>

                <span style="padding-left: 20px; display: inline-block;">
                    <button ng-click="toggleToolPanel()">Toggle Tool Panel</button>
                </span>

                Group Type:
                <select ng-model="groupType" ng-change="onGroupTypeChanged()" style="width: 90px;">
                    <option value="col">Col</option>
                    <option value="colWithFooter">Col with Footer</option>
                    <option value="row">Row</option>
                    <option value="rowWithFooter">Row with Footer</option>
                </select>

                <button ng-click="gridOptions.api.expandAll()">Expand All</button>
                <button ng-click="gridOptions.api.collapseAll()">Collapse All</button>

            </div>
        </div>
        <!-- The table div -->
        <div style="padding: 150px 20px 20px 20px; height: 100%; box-sizing: border-box;">
            <div ag-grid="gridOptions" style="height: 100%;" ng-style="{width: width, height: height}" ng-class="style"></div>
        </div>
    </body>

    <?php include_once("analytics.php"); ?>

</html>
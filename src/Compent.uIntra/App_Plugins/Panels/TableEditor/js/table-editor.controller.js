﻿(function (angular) {
    'use strict';
    const controller = function ($scope, dialogService) {

        var emptyCellModel = '{"value": ""}';
        var defaultModel = {
            useFirstRowAsHeader: true,
            makeFirstColumnBold: false,
            tableStyle: null,
            columnStylesSelected: [
                null,
                null
            ],
            rowStylesSelected: [
                null,
                null,
                null
            ],
            cells: [
                [{ value: "" }, { value: "" }],
                [{ value: "" }, { value: "" }],
                [{ value: "" }, { value: "" }]
            ]
        }

        $scope.restore = function () {
            $scope.control.value = $scope.backupModel;
        };

        $scope.open = function () {
            dialogService.open({
                template: "/App_Plugins/Panels/TableEditor/views/overlay.html",
                show: true,
                modalClass: "panel table-editor",
                dialogData: {
                    canAddColumn: function () {
                        return $scope.canAddColumn();
                    },
                    canRemoveColumn: function () {
                        return $scope.canRemoveColumn();
                    },
                    canAddRow: function () {
                        return $scope.canAddRow();
                    },
                    canRemoveRow: function () {
                        return $scope.canRemoveRow();
                    },
                    canSort: function () {
                        return $scope.canSort();
                    },
                    addColumn: function ($index) {
                        return $scope.addColumn($index);
                    },
                    removeColumn: function ($index) {
                        return $scope.removeColumn($index);
                    },
                    addRow: function ($index) {
                        return $scope.addRow($index);
                    },
                    removeRow: function ($index) {
                        return $scope.removeRow($index);
                    },
                    config: $scope.control.config,
                    model: $scope.control.value,
                    restore: $scope.restore
                },
                closeCallback: function (data) {
                    data.restore && $scope.restore();
                }
            });

            $scope.backupModel = angular.copy($scope.control.value);
        }

        $scope.init = function (control) {
            $scope.control = control;
            $scope.control.config = {
                maxRows: 10,
                maxColumns: 10
            }
            $scope.control.value = $scope.control.value || defaultModel;
        };

        $scope.canAddRow = function () {
            if (isNaN(parseInt($scope.control.config.maxRows, 10))) {
                return true;
            }

            return ($scope.control.config.maxRows > $scope.control.value.cells.length);
        }

        $scope.addRow = function ($index) {
            if ($scope.canAddRow()) {
                var newRow = [];

                for (var i = 0; i < getColumnCount(); i++) {
                    newRow.push(emptyCellModel);
                }

                $scope.control.value.cells.splice($index + 1, 0, JSON.parse("[" + newRow.join(',') + "]"));
            }
        }

        $scope.canAddColumn = function () {
            if (isNaN(parseInt($scope.control.config.maxColumns, 10))) {
                return true;
            }

            return ($scope.control.config.maxColumns > getColumnCount());
        }

        $scope.addColumn = function ($index) {
            if ($scope.canAddColumn()) {

                //style
                $scope.control.value.columnStylesSelected.splice($index + 1, 0, null);

                //cells
                for (var i in $scope.control.value.cells) {
                    $scope.control.value.cells[i].splice($index + 1, 0, JSON.parse(emptyCellModel));
                }
            }
        }

        $scope.canRemoveRow = function () {
            return ($scope.control.value.cells.length > 1);
        }

        $scope.removeRow = function ($index) {
            if ($scope.canRemoveRow()) {
                if (confirm("Are you sure you'd like to remove this row?")) {
                    $scope.control.value.cells.splice($index, 1);
                }
            }
        }

        $scope.canRemoveColumn = function () {
            return getColumnCount() > 1;
        }

        $scope.removeColumn = function ($index) {
            if ($scope.canRemoveColumn()) {
                if (confirm("Are you sure you'd like to remove this column?")) {
                    $scope.control.value.columnStylesSelected.splice($index, 1);

                    for (var i in $scope.control.value.cells) {
                        $scope.control.value.cells[i].splice($index, 1);
                    }
                }
            }
        }

        $scope.canSort = function () {
            return ($scope.control.value.cells.length > 1);
        }

        //sort config
        $scope.sortableOptions = {
            axis: 'y',
            cursor: "move",
            handle: ".handle",
            update: function (ev, ui) {

            },
            stop: function (ev, ui) {

            }
        };

        $scope.clearTable = function () {
            if (confirm("Are you sure you wish to remove everything from the table?")) {
                $scope.control.value = defaultModel;
            }
        }

        function getColumnCount() {
            return $scope.control.value.cells[0].length;
        }
    }

    angular.module('umbraco').controller('TableEditorController', ["$scope", "dialogService", controller]);
})(angular);
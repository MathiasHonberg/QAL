(function () {
  'use strict';

  angular
      .module('forceinspect.genericField.QAL')
      .directive('qalComponentPanel', qalComponentPanel);

  qalComponentPanel.$inject = [];

    function qalComponentPanel() {
        var directive = {
            link: link,
            restrict: 'EA',
            controller: "QalComponentPanelController",
            controllerAs: 'vm',
            scope: {
                genericGroupTypeId: '@',
                itemIdentifier: '=',
                isReadOnly: '@',
                item: '=',
                typeId: '='
            },
            bindToController: true,
            templateUrl: 'app/sections/qal/OBJECT/QAL-ComponentTag.html'
        };

      return directive;

      function link() {
      }
  }
})();
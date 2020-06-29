(function () {
  'use strict';

  angular
      .module('forceinspect.genericField.QAL')
      .directive('qalConfigurationPanel', qalConfigurationPanel);

  qalConfigurationPanel.$inject = [];

  function qalConfigurationPanel() {
      var directive = {
          link: link,
          restrict: 'EA',
          controller: "QalConfigurationPanelController",
          controllerAs: 'vm',
          scope: {
              genericGroupTypeId: '@',
              itemIdentifier: '=',
              typeId: '=',
              isReadOnly: '=',
              item: '=',
              cobj:'=',
              isComponentTag: '@',
              pageMode:'='
          },
         // bindToController: true,
          template: '<ng-include src="getTemplateUrl()"/>'
      };

      return directive;

      function link() {
      }
  }
})();
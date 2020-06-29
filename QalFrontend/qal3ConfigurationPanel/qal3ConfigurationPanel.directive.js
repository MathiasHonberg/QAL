(function () {
  'use strict';

  angular
    .module('forceinspect.genericField.QAL')
    .directive('qal3ConfigurationPanel', qal3ConfigurationPanel);

  qal3ConfigurationPanel.$inject = [];

  function qal3ConfigurationPanel() {
    var directive = {
      link: link,
      restrict: 'EA',
      controller: "Qal3ConfigurationPanelController",
      controllerAs: 'vm',
      scope: {
        genericGroupTypeId: '@',
        itemIdentifier: '=',
        typeId: '=',
        isReadOnly:'='
      },

      //bindToController: true,
      template:'<ng-include src="getTemplateUrl()"/>'
    };

    return directive;

    function link() {
    }
  }
})();
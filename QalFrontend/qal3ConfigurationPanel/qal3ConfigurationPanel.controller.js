(function () {
  'use strict';

  angular
    .module('forceinspect.genericField.QAL')
      .controller('Qal3ConfigurationPanelController', Qal3ConfigurationPanelController);


  Qal3ConfigurationPanelController.$inject = ['$rootScope','$http', '$routeParams', 'authService', 'GRAPH_URL', '$scope', 'breeze', 'queryService',
    'dataService', 'autosaverService', 'qAlGenericConfigDataService', '$timeout', 'QALGRAPH_URL', 'FAL_ENUMS'];

  function Qal3ConfigurationPanelController($rootScope,$http, $routeParams, authService, GRAPH_URL, $scope, breeze, queryService, dataService,
                                            autosaverService, qAlGenericConfigDataService, $timeout, QALGRAPH_URL, FAL_ENUMS) {
    var vm = this;
    vm.ItemId = $scope.itemIdentifier;
    vm.genericGroupTypeId = $scope.genericGroupTypeId;
    vm.typeId = $scope.typeId;
    vm.hasDebugInfoReadPermission = hasDebugInfoReadPermission;
    console.log(vm.ItemId);
    console.log($scope.genericGroupTypeId);
    console.log($scope.typeId);
        
    vm.LocalManager = void 0;
    vm.QALObjectTypes = {
        19: 'QAL-Position',
        20: 'QAL-Meter',
        21: 'QAL-Component'
    };

    vm.QALInspectionTypes = {
        99: 'Quality-Assurance-Level-1',
        100: 'Quality-Assurance-Level-2',
        101: 'Quality-Assurance-Level-3'
    };

    vm.IsReadOnly = '1';
    if ($scope.isReadOnly !== undefined) {
        vm.IsReadOnly=$scope.isReadOnly;
    }

    $scope.getTemplateUrl = function () {
      var folderPath = '';
      var lastPath = '';
      if (vm.GenericGroupType.Object === vm.genericGroupTypeId) {
        folderPath = 'OBJECT';
        lastPath = vm.QALObjectTypes[$scope.typeId];
      } else if (vm.GenericGroupType.Inspection === vm.genericGroupTypeId) {
        folderPath = 'INSPECTION';
        lastPath = vm.QALInspectionTypes[$scope.typeId];
      }

      var link = 'app/sections/qal/' + folderPath + '/' + lastPath + '.html';
      console.log(link);
      return link;
    };

    //vm.ItemId = $routeParams.itemid;
    vm.selectedGenericConfigTableVersion = void 0;
    vm.isNew = false;
    vm.isEdit = false;
    vm.GroupTypeName = '';
    //vm.GroupTypeId = '3';
    vm.itemFieldDesc = 'ItemId';

    //TEST DATA
    //vm.ObjectId = 867124;
    //vm.InspectionKey = 'D40C0813-3EAF-4707-A635-82FD38DE31B2';
    //A59247D0-8F37-4D9E-AD67-6E9414AD3D10
    //vm.ItemId = vm.InspectionKey;
    //END TEST DATA

    vm.GenericConfigTableKey = '00000000-0000-0000-0000-000000000000';
        
    vm.showConfigFieldBlock = false;       
    vm.showVersionMismatchError = false;

    //constant data
    vm.GenericGroupType = {
      Container: '1',
      Object: '2',
      Inspection: '3',
      Report: '4',
      ReportTemplate: '5',
      Item: '6'
    };

    vm.DataType = {
      List: 1,
      Boolean: 2,
      DateTime: 3,
      Decimal: 4,
      Int: 5,
      Text: 6,
      Lookup: 7,
      Item: 8
    };

    vm.genericConfigTable = void 0;
    vm.selectedGenericConfigTable = void 0;
    vm.genericConfigFields = void 0;
        
    vm.ListField = [];
    vm.entities = [];
    vm.entitiesDictionary = {};
       
    vm.GenericLookUpFields = {};
    vm.GenericLookUpTableKeysMapping = {};
    vm.GenericLookUpTableKeysDictionary = {};

    //vm.LookUpMappingTables = [];        
    vm.LookUpFieldsKeysLoaded = {};
    vm.lookupFieldSelectedItem = {};
    vm.lookupSubFieldSelectedItem = {};
    vm.DependentFieldsDictionary = {};
    vm.GenericValueFields = {};

    //function declaration
    vm.getIndexWithTagId = getIndexWithTagId;
    vm.LoadGenericConfigTable = loadGenericConfigTable;
    vm.GetValues = getValues;
    vm.DependentFields = dependentFields;
    vm.GetValueField = GetValueField;
    vm.GetConfigField = GetConfigField; 
    vm.IsEntityExists = IsEntityExists;
    vm.regexEscape = regexEscape;
    vm.ItemKeysMapping = [];
    vm.T_GenericConfigFields = [];
    vm.LocalManager = void 0;
    vm.RecursiveFieldsMapping = [];
    vm.GenericValueFieldsList = [];
    vm.GenericValueRows = [];
    vm.setInspectionToDone = setInspectionToDone;
      vm.getStatusText = getStatusText;
      vm.GenericValueRowKey  = void 0;
      vm.IsGenericValueRowKeyLoadDone = false;

      $scope.$watch('vm.IsGenericValueRowKeyLoadDone', function () {
          if (vm.IsGenericValueRowKeyLoadDone === true) {
              vm.LoadGenericConfigTable();
          }
      });

      vm.genericValueRowKeys = [];
      queryService.delay($scope, 400, loadGenericValueRows);

    function loadGenericValueRows() {
      if (vm.IsReadOnly === true) return;
              var query = new breeze.EntityQuery()
                  .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
                  .where('ItemKey', breeze.FilterQueryOp.Equals, vm.ItemId)
                  .expand('T_GenericValueField');

              dataService.getLocalManager().then(function (manager) {
                  vm.LocalManager = manager;
                  vm.genericConfigurations =
                      queryService.createQueryHelper(
                          qAlGenericConfigDataService.getGenericValueRow(query, manager));
                  vm.genericConfigurations.promise.then(function(data) {
                      
                      var valueRow = data.results[0];
                      if (valueRow !== undefined) {
                          if (valueRow.T_GenericValueField[0] !== undefined) {
                              vm.GenericValueRowKey = valueRow.T_GenericValueField[0].GenericValueFieldKey;
                          }
                      }
                      vm.IsGenericValueRowKeyLoadDone = true;
                  });
                  loadInspection();
              });
      }

      function loadInspection() {
          queryService.createQueryHelper(
                  qAlGenericConfigDataService.getInspections(
                      new breeze.EntityQuery().where('InspectionGUID', breeze.FilterQueryOp.Equals, vm.ItemId),vm.LocalManager))
              .promise.then(function (data) {
                  var inspection = data.results[0];
                  if (inspection !== undefined) {
                      inspection.LastModifiedByInitiatorKey = authService.getUserInfo().InitiatorGuid;
                  }
              });
      }

    function getStatusText(status) {
      var result = '';

      switch (status) {
        case 'NoDrift': result = 'NO_DRIFT'; break;
        case 'ZeroPrecision': result = 'ZERO_PRECISION'; break;
        case 'ZeroPositiveDriftAutomaticInternalAdjustment': result =  'ZERO_POSITIVE_DRIFT_AUTOMATIC_INTERNAL_ADJUSTMENT'; break;
        case 'ZeroNegativeDriftAutomaticInternalAdjustment': result =  'ZERO_NEGATIVE_DRIFT_AUTOMATIC_INTERNAL_ADJUSTMENT'; break;
        case 'ZeroPositiveDriftNonAutomaticInternalAdjustment': result =  'ZERO_POSITIVE_DRIFT_NON_AUTOMATIC_INTERNAL_ADJUSTMENT'; break;
        case 'ZeroNegativeDriftNonAutomaticInternalAdjustment': result =  'ZERO_NEGATIVE_DRIFT_NON_AUTOMATIC_INTERNAL_ADJUSTMENT'; break;
        case 'ZeroPositiveDriftNonAutomaticInternalAdjustmentFirstReading': result =  'ZERO_POSITIVE_DRIFT_NON_AUTOMATIC_INTERNAL_ADJUSTMENT_FIRST_READING'; break;
        case 'ZeroNegativeDriftNonAutomaticInternalAdjustmentFirstReading': result =  'ZERO_NEGATIVE_DRIFT_NON_AUTOMATIC_INTERNAL_ADJUSTMENT_FIRST_READING'; break;
        case 'ZeroUnknownDrift': result =  'ZERO_UNKNOWN_DRIFT'; break;
        case 'SpanPrecision': result =  'SPAN_PRECISION'; break;
        case 'SpanPositiveDriftAutomaticInternalAdjustment': result =  'SPAN_POSITIVE_DRIFT_AUTOMATIC_INTERNAL_ADJUSTMENT'; break;
        case 'SpanNegativeDriftAutomaticInternalAdjustment': result =  'SPAN_NEGATIVE_DRIFT_AUTOMATIC_INTERNAL_ADJUSTMENT'; break;
        case 'SpanPositiveDriftNonAutomaticInternalAdjustment': result =  'SPAN_POSITIVE_DRIFT_NON_AUTOMATIC_INTERNAL_ADJUSTMENT'; break;
        case 'SpanNegativeDriftNonAutomaticInternalAdjustment': result =  'SPAN_NEGATIVE_DRIFT_NON_AUTOMATIC_INTERNAL_ADJUSTMENT'; break;
        case 'SpanPositiveDriftNonAutomaticInternalAdjustmentFirstReading': result =  'SPAN_POSITIVE_DRIFT_NON_AUTOMATIC_INTERNAL_ADJUSTMENT_FIRST_READING'; break;
        case 'SpanNegativeDriftNonAutomaticInternalAdjustmentFirstReading': result =  'SPAN_NEGATIVE_DRIFT_NON_AUTOMATIC_INTERNAL_ADJUSTMENT_FIRST_READING'; break;
        case 'SpanUnknownDrift': result =  'SPAN_UNKNOWN_DRIFT'; break;
        default: result =  ''; break;
      }

      return (result === '' ? result : 'INSPECTION_PROPERTIES.' + result);
    }

    function setInspectionToDone() {
      $('#qalModal').modal('show');
    }

    function loadGenericConfigTable() {
      vm.GenericValueFieldsList = [];
      vm.GenericValueRows = [];

      if (vm.GenericConfigTableKey === undefined)
        return;

      vm.showVersionMismatchError = false;

      dataService.getLocalManager().then(function (manager) {
        vm.LocalManager = manager;
               
        var query = new breeze.EntityQuery()
          .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
          .where('GenericGroupTypeId', breeze.FilterQueryOp.Equals, vm.genericGroupTypeId)
          .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion)
          .where('GroupId', breeze.FilterQueryOp.Equals, vm.typeId)
          .expand('T_GenericConfigField.T_GenericConfigLookupTable.T_GenericConfigLookupField.T_GenericConfigLookupFieldMapping1.T_GenericConfigLookupField');

          vm.genericConfigurations = queryService.createQueryHelper(qAlGenericConfigDataService.getGenericConfigTables(query, manager));
          vm.genericConfigurations.promise.then(load);
      });
    }

    function load(data) {
      vm.genericConfigTable = data.results;
      vm.selectedGenericConfigTable = data.results[0];
      vm.showVersionMismatchError =
        vm.selectedGenericConfigTable === undefined
          || vm.selectedGenericConfigTable.VersionId !== vm.selectedGenericConfigTableVersion;
          
      populateConfigFields();
    }

    function getConfigItemTableConfiguration(genericConfigTableKey) {
      if (vm.GenericConfigTableKey === undefined)
        return;

      vm.showVersionMismatchError = false;
      dataService.getLocalManager().then(function (manager) {
        vm.LocalManager = manager;

        var query = new breeze.EntityQuery()
            .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, genericConfigTableKey)
            .expand('T_GenericConfigField.T_GenericConfigLookupTable.T_GenericConfigLookupField.T_GenericConfigLookupFieldMapping1.T_GenericConfigLookupField,T_GenericValueRow.T_GenericValueField');
        vm.genericConfigurations = queryService.createQueryHelper(qAlGenericConfigDataService.getGenericConfigTables(query, manager));
        vm.genericConfigurations.promise.then(loadGenericFieldsRecursive);
      });
    }

    function loadGenericFieldsRecursive(data) {
        manipulateFields(data.results[0].T_GenericConfigField);
        doOperationOnFields();
    }

    /*
      * populate config field fields
      */
    function populateConfigFields() {
      if (vm.selectedGenericConfigTable === undefined ||
        vm.selectedGenericConfigTable.GenericConfigTableKey === undefined ||
        vm.selectedGenericConfigTable.GenericConfigTableKey === null)
        return;

      vm.GenericValueFields = {}; // Initialize GenericValueField list.
      vm.valueRowKey = void 0;
      vm.genericConfigFields = void 0;
      vm.entities = [];
      vm.cFields = [];

      // Assign T_GenericConfigField for selected configuration.
      vm.T_GenericConfigFields = vm.selectedGenericConfigTable.T_GenericConfigField;

      var valueRows = vm.selectedGenericConfigTable.T_GenericValueRow;

      // This condition determines, whether do we have to insert the data or edit
      if (valueRows === null || valueRows[0] === undefined) {
        vm.genericFieldValues = void 0;
        vm.showConfigFieldBlock = true;
        vm.isNew = true;
      }

        vm.EntryIndex = 0;

        if ((vm.genericGroupTypeId === '3' || vm.genericGroupTypeId === '4')) {
            if (vm.GenericValueRowKey !== undefined)
                checkAndClearEntity(vm.GenericValueRowKey);
            else
                checkAndClearEntity(vm.ItemId);
        }
      manipulateFields(vm.T_GenericConfigFields);

    }

    function manipulateFields(data) {
      //prepare few list, so insted of navigation, we can fetch data faster
      //data is generic config field
      _.forEach(data, function (v) {

        vm.cFields[vm.EntryIndex] = v;
        vm.entitiesDictionary[v.TagId] = vm.EntryIndex++;

        if (v.GenericConfigItemTableKey !== null) {
          console.log(v.GenericConfigItemTableKey);
          getConfigItemTableConfiguration(v.GenericConfigItemTableKey);
        } else {
          if (v.GenericConfigLookupTableKey === null)
            return;

          vm.GenericLookUpTableKeysMapping[v.TagId] =
            v.GenericConfigLookupTableKey; //mapping config fieldkey and lookuptable key

          //mapping lookuptable with posible list of data/LookUpTables
          if (vm.GenericLookUpFields[v.GenericConfigLookupTableKey] === undefined &&
            v.T_GenericConfigLookupTable !== undefined) {
            if (v.T_GenericConfigLookupTable.T_GenericConfigLookupField === null)
              return;

            _.forEach(v.T_GenericConfigLookupTable.T_GenericConfigLookupField,
              function (v1) {
                if (vm.GenericLookUpFields[v1.GenericConfigLookupTableKey] === undefined) {
                  vm.GenericLookUpFields[v1.GenericConfigLookupTableKey] = [];
                }

                vm.GenericLookUpFields[v1.GenericConfigLookupTableKey].push(v1);
              });
            }
        }
      });
    }

    function doOperationOnFields() {
      //select value in dropdown if we have already inserted value in database
      //if (vm.isEdit) {
      //  _.forEach(vm.T_GenericConfigFields, function (fv) {
      //    //vm.lookupFieldSelectedItem[fv.GenericConfigFieldKey] = fv;
      //    if (fv.DataTypeId === vm.DataType.Lookup)
      //      dependentFields(fv.TagId);
      //  });
      //}

      //subscribe auto change
      //if (!vm.isEdit)
      create();
    }
         
    function getIndexWithTagId(tagId) {
      return vm.entitiesDictionary[tagId];
    }

    /*
    * refresh dependent dropdown values based on parent dropdown selected value
    *  GenericLookUpFields - mapping of [GenericLookUpTableKey][List of Lookupfields]
    *  ----T_GenericConfigLookupFieldMapping1 (contains the mapping of dependent child lookupfields)
    *  -------T_GenericConfigLookupField (dependent child lookupfields)
    *  ------------T_GenericConfigLookupTable
    *  -------------------T_GenericConfigField (control in html needs to refresh with value)
    *
    * id is generic config field key
    */
    function dependentFields(id) {
      var fieldValue = vm.entities[vm.getIndexWithTagId(id)];
      if (fieldValue.GenericConfigLookupFieldKey === undefined)
        return;

      var lookUpFieldKey = fieldValue.GenericConfigLookupFieldKey;
      var lookUpTableKey = vm.GenericLookUpTableKeysMapping[id];
      var initialize = true;
      console.log('lookuptable key ' + lookUpTableKey);

      if (vm.GenericLookUpFields[lookUpTableKey] === undefined)
        return;

      _.forEach(vm.GenericLookUpFields[lookUpTableKey], function (v) { //v is lookupfield list for lookuptable or parent lookupfield
        if (v.GenericConfigLookupFieldKey === lookUpFieldKey) {                    
          _.forEach(v.T_GenericConfigLookupFieldMapping1, function (vf) { //dependent or child lookup field
            if (vf.T_GenericConfigLookupField.T_GenericConfigLookupTable!== null) {
              _.forEach(vf.T_GenericConfigLookupField.T_GenericConfigLookupTable.T_GenericConfigField, function (vcf) { //child lookupfield's configfield
                var tagId = vcf.TagId;

                if (vcf.ParentGenericFieldConfigKey.toUpperCase() !== id)
                  return;

                if (vm.DependentFieldsDictionary[tagId] === undefined || initialize) {
                  vm.DependentFieldsDictionary[tagId] = [];
                  initialize = false;
                }

                vm.DependentFieldsDictionary[tagId].push(vf.T_GenericConfigLookupField);
              });
            }
          });
        }
      });

      console.log(vm.DependentFieldsDictionary);
    }

    function getValues(id) {
      return vm.GenericLookUpFields[vm.GenericLookUpTableKeysMapping[id]];
    }

    function regexEscape(decimal) {
      if (decimal === undefined || decimal === null)
        return;

      return decimal.toString().replace(',', '.');
      }

    function create() {
      var createdDatetime = new Date();

      if (vm.selectedGenericConfigTable === undefined)
        return;

      vm.user = authService.getUserInfo();

      if (vm.user === undefined)
        return;

      var valueRow = createValueRow(createdDatetime, vm.ItemId);
      //var i = 0;
      var itemTypes = [];

      _.find(vm.cFields, function (data) {
        if (data.GenericConfigItemTableKey !== null) {
          itemTypes.push(data);
        }
            });

        if (vm.GenericValueRowKey === undefined) {
            itemTypes.forEach(function(configField) {
                if (configField === '')
                    return;

                var item = createValueField(valueRow.GenericValueRowKey,
                    configField.GenericConfigFieldKey,
                    createdDatetime);
                vm.entities[vm.getIndexWithTagId(configField.TagId)] = item;

                var cFields = [];
                vm.cFields.forEach(function(data) {
                    if (data.GenericConfigTableKey === configField.GenericConfigItemTableKey) {
                        cFields.push(data);
                    }
                });

                if (cFields.length > 0) {
                    var itemValueRow = createValueRow(createdDatetime, item.GenericValueFieldKey);
                    cFields.forEach(function(itemConfigField) {
                        if (itemConfigField === '')
                            return;

                        vm.entities[vm.getIndexWithTagId(itemConfigField.TagId)] =
                            createValueField(itemValueRow.GenericValueRowKey,
                                itemConfigField.GenericConfigFieldKey,
                                createdDatetime);
                    });
                }
            });
        } else {
            itemTypes.forEach(function (configField) {
                if (configField === '')
                    return;

                var cFields = [];
                vm.cFields.forEach(function (data) {
                    if (data.GenericConfigTableKey === configField.GenericConfigItemTableKey) {
                        cFields.push(data);
                    }
                });

                if (cFields.length > 0) {
                    //var itemValueRow = createValueRow(createdDatetime, valueRow.GenericValueFieldKey);
                    cFields.forEach(function (itemConfigField) {
                        if (itemConfigField === '')return;
                        vm.entities[vm.getIndexWithTagId(itemConfigField.TagId)] = createValueField(valueRow.GenericValueRowKey, itemConfigField.GenericConfigFieldKey, createdDatetime);
                    });
                }
            });
        }
      

      console.log(vm.cFields);
      console.log(vm.entities);
      }

    function checkAndClearEntity(itemKey) {
        if (itemKey === undefined) return;
        var valueRows = vm.LocalManager.getChanges(['T_GenericValueRow']);
        var valueFields = vm.LocalManager.getChanges(['T_GenericValueField']);

        var currentRowKey = void 0;
        _.find(valueRows, function (vr) {
            if (vr.ItemKey.toUpperCase() === itemKey.toUpperCase()) {
                currentRowKey = vr.GenericValueRowKey;
                vm.genericValueRowKeys.push(vr.GenericValueRowKey);
                vm.LocalManager.detachEntity(vr);
                return;
            }
        });

        var nexRowKey = void 0;
        if (currentRowKey !== undefined) {
            _.forEach(valueFields,
                function (vf) {
                    if (vf.GenericValueRowKey === currentRowKey) {
                        vm.LocalManager.detachEntity(vf);
                        nexRowKey = vf.GenericValueFieldKey;
                    }
                });
        }
        checkAndClearEntity(nexRowKey);
    }

    function createValueRow(createdDatetime, itemId) {
      var valueRow;
      _.find(vm.GenericValueRows, function (data) {
        if (data.GenericConfigTableKey === vm.selectedGenericConfigTable.GenericConfigTableKey
          && data.VersionId === vm.selectedGenericConfigTableVersion) {

          if ((vm.genericGroupTypeId === '3' || vm.genericGroupTypeId === '4') && data.ItemKey === itemId) {
            valueRow = data;
          }
          else if (data.ItemId === itemId) {
            valueRow = data;
          }
        }
      });

      if (valueRow !== undefined)
        return valueRow;

      var LastModifiedBy = vm.user.InitiatorGuid;
      console.log(vm.LocalManager);
      var valueRowKey = breeze.core.getUuid();
      valueRow = vm.LocalManager.createEntity('T_GenericValueRow',
      {
        GenericValueRowKey: valueRowKey,
        GenericConfigTableKey: vm.selectedGenericConfigTable.GenericConfigTableKey,
        VersionId: vm.selectedGenericConfigTableVersion,
        ReadOnly: true,
        LastModifiedBy: LastModifiedBy,
        LastModifiedDate: createdDatetime
      });

      // 3 => Inspection. 4 => Report.
      if ((vm.genericGroupTypeId === '3' || vm.genericGroupTypeId === '4')) {
          if (vm.GenericValueRowKey!==undefined)
              valueRow.ItemKey = vm.GenericValueRowKey;
          else
              valueRow.ItemKey = itemId;
      }

      vm.GenericValueRows.push(valueRow);

      return valueRow;
    }

    function createValueField(valueRowKey, genericConfigFieldKey, createdDatetime) {
      var valueField;
      _.find(vm.GenericValueFieldsList, function (data) {
        if (data.GenericValueRowKey === valueRowKey && data.GenericConfigFieldKey === genericConfigFieldKey) {
          valueField = data;
        }
      });

      if (valueField !== undefined)
        return valueField;

      var genericValueFieldKey = breeze.core.getUuid();
      var LastModifiedBy = vm.user.InitiatorGuid;
      valueField= vm.LocalManager.createEntity('T_GenericValueField',
      {
        GenericValueFieldKey: genericValueFieldKey,
        GenericValueRowKey: valueRowKey,
        GenericConfigFieldKey: genericConfigFieldKey,
        ReadOnly: true,
        LastModifiedDate: createdDatetime,
        LastModifiedBy: LastModifiedBy//'66EFD2FE-3A20-E311-9401-005056A203BC'                                
      });

      vm.GenericValueFieldsList.push(valueField);

      return valueField;
    }

    //get config field index from mapping dictionary

    function GetValueField(id) {
      return vm.entities[vm.getIndexWithTagId(id)];
    }

    function GetConfigField(id) {
      return vm.cFields[vm.getIndexWithTagId(id)];
    }

    function IsEntityExists(id) {
      return vm.entities.length > vm.getIndexWithTagId(id);
    }

    //MAFH TEST - START 
    vm.actionname = $routeParams.actionname;
    vm.number = 1;
    //vm.selectedItemKey = "1C915C7F-3766-42D2-9FF2-FECAB98F373B";
    vm.selectedItemKey = vm.ItemId;
    vm.fromdate = void 0;
    vm.todate = void 0;
    vm.ObjectList = void 0;
    vm.TolerancePercentage = void 0;

    if ($routeParams.number !== undefined) {
      vm.number = $routeParams.number;
    }

    if ($routeParams.ItemKey !== undefined) {
      vm.selectedItemKey = $routeParams.ItemKey;
    }

    if ($routeParams.fromdate !== undefined) {
      vm.fromdate = $routeParams.fromdate;
    }

    if ($routeParams.todate !== undefined) {
      vm.todate = $routeParams.todate;
    }

    vm.readings = void 0;
    vm.traceZero = void 0;
    vm.traceSpan = void 0;

    vm.GraphInfos = {};
    vm.InitialForDebugInfo = false;
    vm.user = authService.getUserInfo();
    vm.DecimalScale = 3;
    vm.DecimalScales = ['_', 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    vm.unit = void 0;
    vm.szero = void 0;
    vm.sspan = void 0;
    vm.graphTestZero = graphTestZero;
    vm.graphTestSpan = graphTestSpan;
    vm.GetReadings = GetReadings;
    graphTestZero();
    graphTestSpan();
    vm.GetReadings();
    //setTimeout(function () {
    //  graphTestSpan();
    //}, 500);
    //setTimeout(function () {
    //  GetReadings();
    //}, 500);
    vm.test = test;
    vm.tracesdata = [];
    //vm.GetReadings();

    function test() {
      document.getElementById('graphContainer').innerHTML = vm.DecimalScale + 'LETS HOPE IT WORKS!';
      //$(".graphContainer").append("<p>Test</p>");
      console.log("TEST!!!");
    }

    function GetReadings() {
      var url = QALGRAPH_URL + '/GetReadings' + '?';
      url += '&ItemKey=' + vm.selectedItemKey;

      authService.getAuthToken().then(function (authToken) {
        var promise = $http({
          method: 'GET',
          // GetGraph is the name of the method in the Webservice GraphController.GetQALTest()
          url: url,
          contentType: "application/json",
          headers: {
            'X-Auth-Token': authToken
          }
        });

        promise.then(function (response) {
          vm.InitialForDebugInfo = vm.user.Username.toLowerCase() === 'mmi' || vm.user.Username.toLowerCase() === 'txa' || vm.user.Username.toLowerCase() === 'mafh';
          vm.ShowChecklist = false;
          // response.data because of something AngularJs does. normally you could just take response and assign it to traces e.g. vm.traces = response.
          vm.readings = void 0;
          vm.readings = response.data;

          if (vm.readings.readings !== undefined)
              vm.unit = vm.readings.readings[0].R_Unit;
            
          showDoneButton();
          //console.log(vm.readings.readings);
        });
      });
    }

    function graphTestZero() {
      var url = QALGRAPH_URL + '/TestWithNewDatabaseZero' + '?';
      url += '&ItemKey=' + vm.selectedItemKey;
      url += '&fromdate=' + vm.fromdate;
      url += '&todate=' + vm.todate;

      authService.getAuthToken().then(function (authToken) {
        var promise = $http({
          method: 'GET',
          // GetGraph is the name of the method in the Webservice GraphController.GetQALTest()
          url: url,
          contentType: "application/json",
          headers: {
            'X-Auth-Token': authToken
          }
        });

        promise.then(function (response) {
          vm.InitialForDebugInfo = vm.user.Username.toLowerCase() === 'mmi' || vm.user.Username.toLowerCase() === 'txa' || vm.user.Username.toLowerCase() === 'mafh';
          vm.ShowChecklist = false;
          // response.data because of something AngularJs does. normally you could just take response and assign it to traces e.g. vm.traces = response.
          vm.traceZero = [];

          //vm.traces = response.data;
          vm.traceZero = response.data.TraceCollection;
          var unit = response.data.Unit;
          var sZero = response.data.SZero;
          console.log(vm.traceZero);
          plotZeroTest('Span', '', '', unit, sZero);
        });
      });

      //plotTest('Span', '', 'ppm');
    }

    function graphTestSpan() {
      var url = QALGRAPH_URL + '/TestWithNewDatabaseSpan' + '?';
      url += '&ItemKey=' + vm.selectedItemKey;
      url += '&fromdate=' + vm.fromdate;
      url += '&todate=' + vm.todate;

      authService.getAuthToken().then(function (authToken) {
        var promise = $http({
          method: 'GET',
          // GetGraph is the name of the method in the Webservice GraphController.GetQALTest()
          url: url,
          contentType: "application/json",
          headers: {
            'X-Auth-Token': authToken
          }
        });

        promise.then(function (response) {
          vm.InitialForDebugInfo = vm.user.Username.toLowerCase() === 'mmi' || vm.user.Username.toLowerCase() === 'txa' || vm.user.Username.toLowerCase() === 'mafh';
          vm.ShowChecklist = false;
          // response.data because of something AngularJs does. normally you could just take response and assign it to traces e.g. vm.traces = response.
          vm.traceSpan = [];

          //vm.traces = response.data;
          vm.traceSpan = response.data.TraceCollection;
          var unit = response.data.Unit;
          var sSpan = response.data.SSpan;
          console.log(vm.traceSpan);
          plotSpanTest('Span     ' + sSpan, '', '', unit);
          //plotSpanTest('Span     ' + vm.traces[0].SSpan, '', '');
        });
      });
      //plotTest('Span', '', 'ppm');
    }

    function plotZeroTest(title, titleX, titleY, unit, sZero) {
      document.getElementById('graphZeroContainer').innerHtml = '';

      // If there are no traces => don't plot anything => just return.
      if (vm.traceZero === null || vm.traceZero.length === 0)
        return;

      var tracesdata = [];

      // Get Unit = 'ppm' / 'mg/m3' / '%'
      //titleY = vm.traces[0].Unit;
      titleY = unit;
      //titleX = vm.traces[0].SZero;

      // Get lowest x value from all the traces. This could be int, decimal, DateTime, string.
      var min_x = vm.traceZero[0].MinX;

      // Get highest x value from all the traces. This could be int, decimal, DateTime, string.
      var max_x = vm.traceZero[0].MaxX;

      // Get lowest y value from all the traces. This could be int, decimal, DateTime, string.
      var min_y = vm.traceZero[0].MinY;

      // Get highest y value from all the traces. This could be int, decimal, DateTime, string.
      var max_y = vm.traceZero[0].MaxY;

      // If the data on the x-axis is DateTime then the
      // layout of the x-axis should be changed.
      var x_axis_is_date = vm.traceZero[0].XIsDateTime;

      // If the data on the y-axis is DateTime then the
      // layout of the y-axis should be changed.
      var y_axis_is_date = vm.traceZero[0].YIsDateTime;

      var trace_tickformat = '10.0f';
      var trace_x_amount = vm.traceZero[0].X.length;
      var trace_y_amount = vm.traceZero[0].Y.length; // TODO find a way to use this.

      // Counter to position each trace in it's own place in the tracesdata array.
      var counter = 0;

      // Put each of the traces into the tracesdata array.
      _.forEach(vm.traceZero, function (trace) {
        if (trace_x_amount < trace.X.length)
          trace_x_amount = trace.X.length;

        // If necessary update min_x.
        if (trace.MinX < min_x)
          min_x = trace.MinX;

        // If necessary update max_x.
        if (trace.MaxX > max_x)
          max_x = trace.MaxX;

        // If necessary update min_y.
        if (trace.MinY < min_y)
          min_y = trace.MinY;

        // If necessary update max_y.
        if (trace.MaxY > max_y)
          max_y = trace.MaxY;

        if (trace.Tickformat)
          trace_tickformat = trace.Tickformat;

        // Add current trace to tracesdata array.
        tracesdata[counter] = {
          x: trace.X, // E.g. [2.5,5,10,15,20,25,30,35]
          y: trace.Y, // E.g. [-0.00025,-0.0005,-0.001,-0.0015,-0.002,-0.0025,-0.003,-0.0035]
          name: trace.Name,
          mode: trace.Mode.toLowerCase() + '+markers', // E.g. lines
          marker: {
            symbol: trace.MarkerStyle.toLowerCase(),
            size: 9
          },
          type: trace.Type.toLowerCase(), // E.g. scatter
          line: {
            dash: trace.LineDash.toLowerCase(), // E.g. dash
            color: 'rgb(' + trace.LineColor + ')', // E.g. 'rgb(255, 128, 0)'
            width: trace.LineWidth // E.g. 1
          }
        };

        // If there are any error Y values then add them to 
        // create a Error bar on the points.
        if (trace.ErrorY.length > 0)
          tracesdata[counter].error_y = {
            type: 'data',
            array: trace.ErrorY,
            visisble: true,
            color: 'rgb(' + trace.LineColor + ')'
          };

        // If the mode has been set to 'markers' then add styling for the marker
        if (trace.Mode.toLowerCase() === 'markers')
          tracesdata[counter].marker = {
            color: 'rgb(' + trace.LineColor + ')',
            size: 8
          };

        counter++;
      });

      if (counter === 5 || counter === 6) {
        title = 'Zero     ' + sZero;
      }

      min_y = min_y + (min_y / 20);
      max_y = max_y + (max_y / 20);

      // Calculate the distance between ticks on the y-axis.
      var amount_of_y_ticks = trace_y_amount;
      var dtick_y = (max_y - min_y) / amount_of_y_ticks;

      var layout = {
        separators: ',.', // First sign is the decimal separator. Second sign is the thousands separator. So ',.' should be used for danish graphs.
        title: // Set title of the graph.
        {
          text: '<b>' + title + '</b>',
          font: {
            family: 'Courier New, monospace',
            size: 25
          }
        },
        xaxis: {
          title: '<b>' + titleX + '</b>', // Title placed below the x-axis.
          font: {
            family: 'Courier New, monospace',
            size: 8
          },
          automargin: true, // true => Ensure that the x-axis text/header doesn't collide with the tick text.
          showline: true, // true => Show x-axis.
          autotick: false, // false => Use custom format specified in this section.
          tickformat: '1.2f', // 1.2f => Amount of decimals for ticks on the x-axis is 2.
          ticks: 'outside', // outside => Place ticks outside of the plotted area, i.e. below the x-axis.
          ticklen: 8, // Length of the tick on the x-axis.
          tickwidth: 1, // Width of the tick on the x-axis.
          //   rangemode: 'tozero' // tozero => Expand x-axis to zero even if none of the traces have points that touches in x = 0.
        },
        yaxis: {
          title: '<b>' + titleY + '</b>',
          showline: true,
          autotick: false, // false => Use custom format specified in this section.
          tickformat: trace_tickformat,//'1.6f', // 1.2f => Amount of decimals for ticks on the y-axis is 3.
          ticks: 'outside', // outside => Place ticks outside of the plotted area, i.e. to the left of the y-axis.
          dtick: dtick_y, //0.001, // Distance between each tick on the y-axis.
          ticklen: 8, // Length of the tick on the y-axis.
          tickwidth: 1, // Width of the tick on the y-axis.
          range: [min_y, max_y], // Set range to ensure that range will not change if the user hides any of the traces.
          //rangemode: 'tozero' // tozero => Expand y-axis to zero even if none of the traces have points that touches in y = 0.
        }
      };

      // X-axis
      if (x_axis_is_date === true) {
        layout.xaxis.tickformat = '%B %Y';
        layout.xaxis.dtick = 'M2';
        layout.xaxis.tickangle = 270;

        // Convert to Date so it is possible to use the getMonth function correctly.
        var dx_min = new Date(min_x);
        var dx_max = new Date(max_x);

        // Set min and max to -1 and +1 month.
        min_x = dx_min.setMonth(dx_min.getMonth() - 1);
        max_x = dx_max.setMonth(dx_max.getMonth() + 1);

        layout.xaxis.range = [min_x, max_x]; // Set range to ensure that range will not change if the user hides any of the traces.
      }
      else {
        layout.xaxis.tickformat = '%m-%b-%Y';
        //layout.xaxis.dtick = Math.round(Math.ceil(max_x - min_x) / trace_x_amount); // Distance between each tick on the x-axis.
        layout.xaxis.dtick = 'M4'; // Distance between each tick on the x-axis.
        //layout.xaxis.range = ['2013-01-04', '2018-04-01']; // Set range to ensure that range will not change if the user hides any of the traces.
        layout.xaxis.range = [min_x, max_x]; // Set range to ensure that range will not change if the user hides any of the traces.
      }

      // Y-axis
      if (y_axis_is_date === true) {
        layout.yaxis.tickformat = '%B %Y';
        layout.yaxis.dtick = 'M2';
        layout.yaxis.tickangle = 270; // TODO Change angle.

        // Convert to Date so it is possible to use the getMonth function correctly.
        var dy_min = new Date(min_y);
        var dy_max = new Date(max_y);

        // Set min and max to -1 and +1 month.
        min_y = dy_min.setMonth(dy_min.getMonth() - 1);
        max_y = dy_max.setMonth(dy_max.getMonth() + 1);
        layout.yaxis.range = dtick_y; // Set range to ensure that range will not change if the user hides any of the traces.
      }
      else {
        layout.yaxis.tickformat = '1.1f';
        //This doesn't seem to work correctly.
        //layout.yaxis.dtick = Math.round(Math.ceil(max_y - min_y) / trace_y_amount); // Distance between each tick on the y-axis.
        layout.yaxis.dtick = dtick_y; //0.001, // Distance between each tick on the y-axis.
        //layout.yaxis.range = [(min_y * 0.95), (max_y * 1.05)]; // Where the min_y point and max_y point will be in the y-axis
        layout.yaxis.range = dtick_y; // Where the min_y point and max_y point will be in the y-axis
      }

      // Plot the graph.
      Plotly.newPlot(document.getElementById('graphZeroContainer'), tracesdata, layout, { displaylogo: false }, { locale: 'da' });
    }

    function plotSpanTest(title, titleX, titleY, unit) {
      document.getElementById('graphSpanContainer').innerHtml = '';

      // If there are no traces => don't plot anything => just return.
      if (vm.traceSpan === null || vm.traceSpan.length === 0)
        return;

      var tracesdata = [];

      // Get Unit = 'ppm' / 'mg/m3' / '%'
      //titleY = vm.traces[0].Unit;
      //titleY = vm.traces[0].Unit;
      titleY = unit;
      //titleX = vm.traces[0].SSpan;

      // Get lowest x value from all the traces. This could be int, decimal, DateTime, string.
      var min_x = vm.traceSpan[0].MinX;

      // Get highest x value from all the traces. This could be int, decimal, DateTime, string.
      var max_x = vm.traceSpan[0].MaxX;

      // Get lowest y value from all the traces. This could be int, decimal, DateTime, string.
      var min_y = vm.traceSpan[0].MinY;

      // Get highest y value from all the traces. This could be int, decimal, DateTime, string.
      var max_y = vm.traceSpan[0].MaxY;

      // If the data on the x-axis is DateTime then the
      // layout of the x-axis should be changed.
      var x_axis_is_date = vm.traceSpan[0].XIsDateTime;

      // If the data on the y-axis is DateTime then the
      // layout of the y-axis should be changed.
      var y_axis_is_date = vm.traceSpan[0].YIsDateTime;

      var trace_tickformat = '10.0f';
      var trace_x_amount = vm.traceSpan[0].X.length;
      var trace_y_amount = vm.traceSpan[0].Y.length; // TODO find a way to use this.

      // Counter to position each trace in it's own place in the tracesdata array.
      var counter = 0;

      // Put each of the traces into the tracesdata array.
      _.forEach(vm.traceSpan, function (trace) {
        if (trace_x_amount < trace.X.length)
          trace_x_amount = trace.X.length;

        // If necessary update min_x.
        if (trace.MinX < min_x)
          min_x = trace.MinX;

        // If necessary update max_x.
        if (trace.MaxX > max_x)
          max_x = trace.MaxX;

        // If necessary update min_y.
        if (trace.MinY < min_y)
          min_y = trace.MinY;

        // If necessary update max_y.
        if (trace.MaxY > max_y)
          max_y = trace.MaxY;

        if (trace.Tickformat)
          trace_tickformat = trace.Tickformat;

        // Add current trace to tracesdata array.
        tracesdata[counter] = {
          x: trace.X, // E.g. [2.5,5,10,15,20,25,30,35]
          y: trace.Y, // E.g. [-0.00025,-0.0005,-0.001,-0.0015,-0.002,-0.0025,-0.003,-0.0035]
          name: trace.Name,
          mode: trace.Mode.toLowerCase() + '+markers', // E.g. lines
          marker: {
            symbol: trace.MarkerStyle.toLowerCase(),
            size: 9
          },
          type: trace.Type.toLowerCase(), // E.g. scatter
          line: {
            dash: trace.LineDash.toLowerCase(), // E.g. dash
            color: 'rgb(' + trace.LineColor + ')', // E.g. 'rgb(255, 128, 0)'
            width: trace.LineWidth // E.g. 1
          }
        };

        // If there are any error Y values then add them to 
        // create a Error bar on the points.
        if (trace.ErrorY.length > 0)
          tracesdata[counter].error_y = {
            type: 'data',
            array: trace.ErrorY,
            visisble: true,
            color: 'rgb(' + trace.LineColor + ')'
          };

        // If the mode has been set to 'markers' then add styling for the marker
        if (trace.Mode.toLowerCase() === 'markers')
          tracesdata[counter].marker = {
            color: 'rgb(' + trace.LineColor + ')',
            size: 8
          };

        counter++;
      });

      if (counter === 5) {
        title = 'Zero';
      }

      min_y = min_y + (min_y / 20);
      max_y = max_y + (max_y / 20);

      // Calculate the distance between ticks on the y-axis.
      var amount_of_y_ticks = trace_y_amount;
      var dtick_y = (max_y - min_y) / amount_of_y_ticks;

      var layout = {
        separators: ',.', // First sign is the decimal separator. Second sign is the thousands separator. So ',.' should be used for danish graphs.
        title: { // Set title of the graph.
          text: '<b>' + title + '</b>',
          font: {
            family: 'Courier New, monospace',
            size: 25
          }
        },
        xaxis: {
          title: '<b>' + titleX + '</b>', // Title placed below the x-axis.
          font: {
            family: 'Courier New, monospace',
            size: 8
          },
          automargin: true, // true => Ensure that the x-axis text/header doesn't collide with the tick text.
          showline: true, // true => Show x-axis.
          autotick: false, // false => Use custom format specified in this section.
          tickformat: '1.2f', // 1.2f => Amount of decimals for ticks on the x-axis is 2.
          ticks: 'outside', // outside => Place ticks outside of the plotted area, i.e. below the x-axis.
          ticklen: 8, // Length of the tick on the x-axis.
          tickwidth: 1, // Width of the tick on the x-axis.
          //   rangemode: 'tozero' // tozero => Expand x-axis to zero even if none of the traces have points that touches in x = 0.
        },
        yaxis: {
          title: '<b>' + titleY + '</b>',
          showline: true,
          autotick: false, // false => Use custom format specified in this section.
          tickformat: trace_tickformat,//'1.6f', // 1.2f => Amount of decimals for ticks on the y-axis is 3.
          ticks: 'outside', // outside => Place ticks outside of the plotted area, i.e. to the left of the y-axis.
          dtick: dtick_y, //0.001, // Distance between each tick on the y-axis.
          ticklen: 8, // Length of the tick on the y-axis.
          tickwidth: 1, // Width of the tick on the y-axis.
          range: [min_y, max_y], // Set range to ensure that range will not change if the user hides any of the traces.
          //rangemode: 'tozero' // tozero => Expand y-axis to zero even if none of the traces have points that touches in y = 0.
        }
      };

      // X-axis
      if (x_axis_is_date === true) {
        layout.xaxis.tickformat = '%B %Y';
        layout.xaxis.dtick = 'M2';
        layout.xaxis.tickangle = 270;

        // Convert to Date so it is possible to use the getMonth function correctly.
        var dx_min = new Date(min_x);
        var dx_max = new Date(max_x);

        // Set min and max to -1 and +1 month.
        min_x = dx_min.setMonth(dx_min.getMonth() - 1);
        max_x = dx_max.setMonth(dx_max.getMonth() + 1);

        layout.xaxis.range = [min_x, max_x]; // Set range to ensure that range will not change if the user hides any of the traces.
      }
      else {
        layout.xaxis.tickformat = '%m-%b-%Y';
        //layout.xaxis.dtick = Math.round(Math.ceil(max_x - min_x) / trace_x_amount); // Distance between each tick on the x-axis.
        layout.xaxis.dtick = 'M4'; // Distance between each tick on the x-axis.
        //layout.xaxis.range = ['2013-01-04', '2018-04-01']; // Set range to ensure that range will not change if the user hides any of the traces.
        layout.xaxis.range = [min_x, max_x]; // Set range to ensure that range will not change if the user hides any of the traces.
      }

      // Y-axis
      if (y_axis_is_date === true) {
        layout.yaxis.tickformat = '%B %Y';
        layout.yaxis.dtick = 'M2';
        layout.yaxis.tickangle = 270; // TODO Change angle.

        // Convert to Date so it is possible to use the getMonth function correctly.
        var dy_min = new Date(min_y);
        var dy_max = new Date(max_y);

        // Set min and max to -1 and +1 month.
        min_y = dy_min.setMonth(dy_min.getMonth() - 1);
        max_y = dy_max.setMonth(dy_max.getMonth() + 1);

        layout.yaxis.range = dtick_y; // Set range to ensure that range will not change if the user hides any of the traces.
      }
      else {
        layout.yaxis.tickformat = '1.1f';
        //This doesn't seem to work correctly.
        //layout.yaxis.dtick = Math.round(Math.ceil(max_y - min_y) / trace_y_amount); // Distance between each tick on the y-axis.
        layout.yaxis.dtick = dtick_y; //0.001, // Distance between each tick on the y-axis.
        //layout.yaxis.range = [(min_y * 0.95), (max_y * 1.05)]; // Where the min_y point and max_y point will be in the y-axis
        layout.yaxis.range = dtick_y; // Where the min_y point and max_y point will be in the y-axis
      }

      // Plot the graph.
      Plotly.newPlot(document.getElementById('graphSpanContainer'), tracesdata, layout, { displaylogo: false }, { locale: 'da' });
    }
    //MAFH TEST - END

      function showDoneButton() {

          vm.ShowMakeDoneButton = false;

          if (vm.readings.readings === undefined) {
            $rootScope.$broadcast('QAL3.SetToDone', {
                data: { QAL3SetToDone: vm.ShowMakeDoneButton, hasQalInfo: vm.IsReadOnly }
            });
            return;
          }

          var newestReadingStatus = vm.readings
              .readings[vm.readings.readings.length-1]
              .Status;

          vm.ShowMakeDoneButton = newestReadingStatus !== 'NoDrift';
        vm.IsReadOnly = newestReadingStatus !== 'NoDrift';

        $rootScope.$broadcast('QAL3.SetToDone', {
            //data: { QAL3SetToDone: vm.ShowMakeDoneButton }
          data: { QAL3SetToDone: vm.ShowMakeDoneButton, hasQalInfo: vm.IsReadOnly }
          });
      }

      function hasDebugInfoReadPermission() {
        return (authService.hasPermission(FAL_ENUMS.PERMISSION.DEBUG_INFO_READ))
      }
  }
})();
(function () {
  'use strict';

  angular
    .module('forceinspect.genericField.QAL')
      .controller('QalGenericFieldController', QalGenericFieldController);


  QalGenericFieldController.$inject = ['$scope', '$routeParams', 'breeze', 'queryService','authService','dataService','qAlGenericConfigDataService'];

    function QalGenericFieldController($scope, $routeParams, breeze, queryService, authService,dataService, qAlGenericConfigDataService)
    {
        var vm = this;
        vm.ItemId = $routeParams.itemid;
        vm.selectedGenericConfigTableVersion = void 0;
        vm.isNew = false;
        vm.isEdit = false;
        vm.GroupTypeName = '';
        vm.GroupTypeId = '3';
        vm.itemFieldDesc = 'ItemId';

        //TEST DATA
        vm.ObjectId = 867124;
        vm.InspectionKey = 'E20C6432-C484-445D-9633-8B33BDBAB0DD';
        vm.ItemId = vm.InspectionKey;
        //END TEST DATA

        vm.GenericConfigTableKey = '00000000-0000-0000-0000-000000000000';
        

        vm.showConfigFieldBlock = false;       
        vm.showVersionMismatchError = false;

        //constant data
        vm.GenericGroupType = {
            Container: '1',
            Object: '2',
            Inspection: '3',
            Report: '4'
        };

        vm.DataType = {
            List: 1,
            Boolean: 2,
            DateTime: 3,
            Decimal: 4,
            Int: 5,
            Text: 6,
            Lookup:7
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
        vm.saveGenericValueFields = saveGenericValueFields;
        vm.insertNewGenericValueFields = insertNewGenericValueFields;        
        vm.DependentFields = dependentFields;
        

        queryService.delay($scope, 400, vm.LoadGenericConfigTable);

        /*
         * Load configurations
         */

     function loadGenericConfigTable() {
         if (vm.GenericConfigTableKey === undefined) return;
         vm.showVersionMismatchError = false;
         var query = new breeze.EntityQuery()
             .where('GenericConfigTableKey', breeze.FilterQueryOp.Equals, vm.GenericConfigTableKey)
             .where('VersionId', breeze.FilterQueryOp.Equals, vm.selectedGenericConfigTableVersion )
             .expand('T_GenericConfigField.T_GenericConfigLookupTable.T_GenericConfigLookupField.T_GenericConfigLookupFieldMapping1.T_GenericConfigLookupField,T_GenericValueRow.T_GenericValueField');
          var promise = qAlGenericConfigDataService.getGenericConfigTables(query);
          promise.then(function(data) {
              vm.genericConfigTable = data.results;
              vm.selectedGenericConfigTable = data.results[0];
              vm.showVersionMismatchError =
                  vm.selectedGenericConfigTable === undefined
                  || vm.selectedGenericConfigTable.VersionId !== vm.selectedGenericConfigTableVersion;
               
              populateConfigFields();
          });
      }


        /*
         * populate config field fields
         */

        function populateConfigFields() {
            if (vm.selectedGenericConfigTable === undefined ||
                vm.selectedGenericConfigTable.GenericConfigTableKey === undefined ||
                vm.selectedGenericConfigTable.GenericConfigTableKey === null) return;

            vm.GenericValueFields = {}; //initialize GenericValueField list
            vm.valueRowKey = void 0;
            vm.genericConfigFields = void 0;
            vm.entities = [];

            //assign T_GenericConfigField for selected configuration
            var data = vm.selectedGenericConfigTable.T_GenericConfigField;

            var valueRows = vm.selectedGenericConfigTable.T_GenericValueRow;

            //this condition determines, wheather do we have to insert the data or edit
            if (valueRows === null || valueRows[0] === undefined) {
                vm.genericFieldValues = void 0;
                vm.showConfigFieldBlock = true;
                vm.isNew = true;
                vm.IsEdit = false;
            } else {
                vm.valueRowKey = valueRows[0].GenericValueRowKey;
                vm.GenericValueFields = valueRows[0].T_GenericValueField;
                vm.showConfigFieldBlock = false;
                vm.isEdit = true;
                vm.isNew = false;
            }

            var i = 0;
            //print(data, 'populateConfigFields');

            //prepare few list, so insted of navigation, we can fetch data faster
            _.forEach(data, function (v) {
                
                if (vm.isEdit && (v.DataType === vm.DataType.List
                    || v.DataTypeId === vm.DataType.Boolean
                    || v.DataTypeId === vm.DataType.DateTime
                    || v.DataTypeId === vm.DataType.Decimal
                    || v.DataTypeId === vm.DataType.Int
                    || v.DataTypeId === vm.DataType.Text)) {
                    vm.entities[i] = v.T_GenericValueField[0];
                    vm.entities[i].Name = v.Name;
                } else {
                    vm.entities[i] = v;
                }

                vm.entitiesDictionary[v.TagId] = i++;
                if (v.GenericConfigLookupTableKey === null) return;                
                vm.GenericLookUpTableKeysMapping[v.GenericConfigFieldKey] = v.GenericConfigLookupTableKey;

                //mapping lookuptable with posible list of data/LookUpTables
                if (vm.GenericLookUpFields[v.GenericConfigLookupTableKey] === undefined && v.T_GenericConfigLookupTable !== undefined) {
                    if (v.T_GenericConfigLookupTable.T_GenericConfigLookupField === null) return;
                    _.forEach(v.T_GenericConfigLookupTable.T_GenericConfigLookupField, function (v1) {
                        if (vm.GenericLookUpFields[v1.GenericConfigLookupTableKey] === undefined) {
                            vm.GenericLookUpFields[v1.GenericConfigLookupTableKey] = [];
                        }
                        vm.GenericLookUpFields[v1.GenericConfigLookupTableKey].push(v1);
                    });
                }

            });

           // print(console.log(vm.GenericLookUpFields), 'lf');

            //this list has been usede to get configfield list/retrive name in UI, also used during insert new value
            //vm.entities = data;
            vm.entities[i] = '';
            //print(vm.entities);

            //select value in dropdown if we have already inserted value in database
            if (vm.isEdit) {
                _.forEach(vm.GenericValueFields, function (fv) {
                    vm.lookupFieldSelectedItem[fv.GenericConfigFieldKey] = fv;
                    dependentFields(fv.GenericConfigFieldKey);
                });
            }
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
       */
      function dependentFields(id) {
            console.log(vm.lookupFieldSelectedItem[id]);
          var lookUpFieldKey = vm.lookupFieldSelectedItem[id].GenericConfigLookupFieldKey;            
            var lookUpTableKey = vm.GenericLookUpTableKeysMapping[id];
            var initialize = true;
            console.log('lookuptable key ' + lookUpTableKey);

            if (vm.GenericLookUpFields[lookUpTableKey] === undefined) return;
            _.forEach(vm.GenericLookUpFields[lookUpTableKey], function (v) {
                if (v.GenericConfigLookupFieldKey === lookUpFieldKey) {                    
                    _.forEach(v.T_GenericConfigLookupFieldMapping1, function (vf) {
                        if (vf.T_GenericConfigLookupField.T_GenericConfigLookupTable!== null) {
                        _.forEach(vf.T_GenericConfigLookupField.T_GenericConfigLookupTable.T_GenericConfigField, function (vcf) {
                            var configFieldKey = vcf.GenericConfigFieldKey;
                            if (vcf.ParentGenericFieldConfigKey !== id) return;
                            if (vm.DependentFieldsDictionary[configFieldKey] === undefined || initialize) {
                                vm.DependentFieldsDictionary[configFieldKey] = [];
                                initialize = false;
                            }

                            vm.DependentFieldsDictionary[configFieldKey].push(vf.T_GenericConfigLookupField);
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

        function saveGenericValueFields() {
           if (vm.isNew) {
               insertNewGenericValueFields();
           }
           else if (vm.isEdit) {
               dataService.saveDefaultManager();
           }
        }


        function insertNewGenericValueFields() {
            var createdDatetime = new Date();
            if (vm.selectedGenericConfigTable === undefined) return;
            vm.user = authService.getUserInfo();
            if (vm.user === undefined) return;
            var LastModifiedBy = vm.user.InitiatorGuid;
            dataService.getDefaultManager().then(function (manager) {
                console.log(manager);
                var valueRowKey = breeze.core.getUuid();

                var valueRow = manager.createEntity('T_GenericValueRow',
                    {
                        GenericValueRowKey: valueRowKey,
                        GenericConfigTableKey: vm.selectedGenericConfigTable.GenericConfigTableKey,
                        VersionId: vm.selectedGenericConfigTableVersion,
                        ReadOnly: true,
                        LastModifiedBy: LastModifiedBy,
                        LastModifiedDate: createdDatetime
                    });

                // 3 => Inspection. 4 => Report.
                if (vm.GroupTypeId === '3' || vm.GroupTypeId === '4') {
                    valueRow.ItemKey = vm.ItemId;
                } else {
                    valueRow.ItemId = vm.ItemId;
                }

                vm.entities.forEach(function (element) {
                    if (element === '') return;
                    var genericValueFieldKey = breeze.core.getUuid();
                    var fieldRow = manager.createEntity('T_GenericValueField',
                        {
                            GenericValueFieldKey: genericValueFieldKey,
                            GenericValueRowKey: valueRowKey,
                            GenericConfigFieldKey: element.GenericConfigFieldKey,
                            ReadOnly: true,
                            LastModifiedDate: createdDatetime,
                            LastModifiedBy: LastModifiedBy//'66EFD2FE-3A20-E311-9401-005056A203BC'                                
                        });

                    if (element.DataTypeId === vm.DataType.Boolean) {
                        fieldRow.ValueBit = element.ValueBit;
                    }
                    else if (element.DataTypeId === vm.DataType.DateTime) {
                        fieldRow.ValueDateTimeOffset = element.ValueDateTimeOffset;
                    }
                    else if (element.DataTypeId === vm.DataType.Decimal) {
                        fieldRow.ValueDecimal = regexEscape(element.ValueDecimal);
                    }
                    else if (element.DataTypeId === vm.DataType.Int) {
                        fieldRow.ValueInt = element.ValueInt;
                    }
                    else if (element.DataTypeId === vm.DataType.Text) {
                        fieldRow.ValueNvarchar = element.ValueNvarchar;
                    } else if (element.DataTypeId === vm.DataType.Lookup) {
                        var lookupFieldKey = vm.lookupFieldSelectedItem[element.GenericConfigFieldKey].GenericConfigLookupFieldKey;
                        fieldRow.GenericConfigLookupFieldKey = lookupFieldKey;
                    }
                    console.log('datatype ' + element.DataTypeId);
                });
                try {
                    dataService.save(manager);
                    vm.isNew = false;
                    vm.isEdit = true;
                } catch (err) {
                    vm.isNew = true;
                }
            });
        }

        function regexEscape(decimal) {
            if (decimal === undefined || decimal===null) return;
            return decimal.replace(',', '.');
        }

        //function print(data, diff) {
        //    console.log('--------- start' + diff + '---------------------');
        //    console.log(data);
        //    console.log('--------- end' + diff + '---------------------');
        //}

      //get config field index from mapping dictionary
  }

})();

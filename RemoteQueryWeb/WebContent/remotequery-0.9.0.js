(function(root) {

  var url = 'remoteQuery';
  var REMOTE_QUERY_NAME = root['REMOTE_QUERY_NAME'];

  if (typeof REMOTE_QUERY_NAME !== 'string') {
    REMOTE_QUERY_NAME = 'rQ';
  }

  root[REMOTE_QUERY_NAME] = {
    'call' : callRq,
    'callForm' : callRqForm,
    'url' : function(arg0) {
      if (typeof arg0 === 'string') {
        url = arg0;
      }
      return url;
    },
    'names' : {
      'RegisterService' : 'RegisterService'
    },
    'toList' : toList,
    'toMap' : toMap
  };

  function callRq(serviceId, arg1, arg2) {
    var parameters, callback;
    if (typeof arg1 === 'function') {
      parameters = {};
      callback = arg1;
    } else {
      parameters = arg1;
      callback = arg2;
    }

    $.ajax({
      'url' : url + '/' + serviceId,
      'dataType' : 'json',
      'data' : parameters,
      'async' : true,
      'cache' : false,
      'type' : 'POST',
      'success' : function() {
        if (callback !== undefined) {
          callback.apply(this, arguments);
        }
      }
    });
  }

  function callRqForm(form$, serviceId, arg2, arg3) {
    var params, cb;
    if (typeof arg3 === 'function') {
      cb = arg3;
      params = arg2;
    } else {
      params = {};
      cb = arg2;
    }
    form$.attr('enctype', 'multipart/form-data');
    form$.ajaxSubmit({
      'url' : url + '/' + serviceId,
      'dataType' : 'json',
      'data' : params,
      'clearForm' : false,
      'type' : 'POST',
      'error' : function(e) {
        alert('error ' + e);
      },
      'success' : function(data) {
        cb(data);
      }
    });
  }

  function callRqMulti(requestArray, mainCb) {
    var requestArrayStr = JSON.stringify(requestArray);
    callRq('MultiService', {
      'requestArray' : requestArrayStr
    }, processSuccess);

    function processSuccess(data) {
      var resultArray = [];
      var requestCb, pr;
      if (data.table && data.table.length === requestArray.length) {
        $.each(data.table, function(i, row) {
          pr = JSON.parse(row[0]);
          resultArray.push(pr);
        });
        if (_.isFunction(mainCb)) {
          mainCb(resultArray);
        }
      }
    }
  }

  function toList(serviceData) {
    var list;
    list = [];
    if (_.isObject(serviceData) && _.isArray(serviceData.table)) {
      $.each(serviceData.table, function(rowIndex, row) {
        var obj = {};
        list.push(obj);
        $.each(serviceData.header, function(colIndex, head) {
          obj[head] = row[colIndex];
        });
      });
      list.header = serviceData.header;
    }
    return list;
  }

  function toMap(serviceData, keyColumns) {
    var map = {}, keys, keyIndexes = [], i;
    var rowCounter, row, currentMap;
    if (_.isArray(keyColumns)) {
      keys = keyColumns;
    } else {
      keys = keyColumns.split('.');
    }
    if (serviceData.table && serviceData.header) {
      for (i = 0; i < keys.length; i++) {
        var keyColumn = keys[i];
        $.each(serviceData.header, function(index, headerValue) {
          if (headerValue == keyColumn) {
            keyIndexes.push(index);
          }
        });
      }
      // new and fast
      for (rowCounter = 0; rowCounter < serviceData.table.length; rowCounter++) {
        row = serviceData.table[rowCounter];
        currentMap = map;
        for (i = 0; i < keys.length; i++) {
          var keyIndex = keyIndexes[i];
          var keyName = row[keyIndex];
          if (!currentMap[keyName]) {
            currentMap[keyName] = {};
          }
          currentMap = currentMap[keyName];
        }
        for (i = 0; i < serviceData.header.length; i++) {
          currentMap[serviceData.header[i]] = row[i];
        }
      }
    }
    return map;
  }

})(this);

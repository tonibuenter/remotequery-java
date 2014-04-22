(function(root) {

  var url = 'remoteQuery';
  var REMOTE_QUERY_NAME = root['REMOTE_QUERY_NAME'];

  if (typeof REMOTE_QUERY_NAME !== 'string') {
    REMOTE_QUERY_NAME = 'rQ';
  }

  root[REMOTE_QUERY_NAME] = {
    'call' : callRq,
    'url' : function(arg0) {
      if (typeof arg0 === 'string') {
        url = arg0;
      }
      return url;
    },
    'names' : {
      'RegisterService' : 'RegisterService'
    }
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
        if (callback) {
          callback.apply(this, arguments);
        }
      }
    });
  }

})(this);

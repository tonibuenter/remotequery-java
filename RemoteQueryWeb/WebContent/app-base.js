var APP_base = APP_base || {};

//
// init function for alle APP_ui functions
//

(function() {

  var counter = 100;

  function newId() {
    return counter++;
  }
  APP_base.newId = newId;
})();

var rQ = rQ || {};

(function() {

  var counter = 0;
  //
  // NEW ID
  //
  function newId(name) {
    counter++;
    return name ? name + counter : '' + counter;
  }
  rQ.newId = newId;

})();
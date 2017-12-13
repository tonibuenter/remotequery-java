var rQ_base = rQ_base || {};

(function() {

  var counter = 0;
  //
  // NEW ID
  //
  /**
   * @memberOf rQ_base
   */
  function newId(name) {
    counter++;
    return name ? name + counter : '' + counter;
  }
  rQ_base.newId = newId;
  // NEW ID

})();
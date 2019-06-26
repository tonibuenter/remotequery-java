function hy(componentOrTag, properties, children) {
  var r;
  if (typeof componentOrTag == 'string') {
    r = $('<' + componentOrTag + '>', properties);
  } else if (typeof componentOrTag == 'function') {
    r = componentOrTag(properties);
  }
  if (r instanceof $) {
    if (typeof children == 'string') {
      r.text(children);
    } else if (typeof children == 'object' && children.length) {
      for (var i = 0; i < children.length; i++) {
        r.append(h);
      }
    }
  }
}
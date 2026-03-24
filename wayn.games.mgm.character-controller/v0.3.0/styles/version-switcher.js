(function () {
  'use strict';

  function init() {
    var container = document.querySelector('.version-switcher');
    if (!container) return;

    var select = container.querySelector('select');
    if (!select) return;

    // _rel is injected by scripts.tmpl.partial into window.__docfx
    var docfx = window.__docfx || {};
    var rel = docfx._rel || '';
    var current = docfx._packageVersion || '';

    var xhr = new XMLHttpRequest();
    xhr.open('GET', rel + 'versions.json', true);
    xhr.onload = function () {
      if (xhr.status !== 200 && xhr.status !== 0) return;
      var versions;
      try { versions = JSON.parse(xhr.responseText); } catch (e) { return; }
      if (!Array.isArray(versions) || versions.length === 0) return;

      versions.forEach(function (v) {
        var opt = document.createElement('option');
        opt.value = v.url;
        opt.textContent = 'v' + v.version;
        if (v.version === current) {
          opt.selected = true;
        }
        select.appendChild(opt);
      });

      select.addEventListener('change', function () {
        if (select.value) {
          window.location.href = select.value;
        }
      });
    };
    xhr.send();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();

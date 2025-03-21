/*! iFrame Resizer (iframeSizer.min.js ) - v4.2.11 - 2020-06-02
 *  Desc: Force cross domain iframes to size to content.
 *  Requires: iframeResizer.contentWindow.min.js to be loaded into the target frame.
 *  Copyright: (c) 2020 David J. Bradshaw - dave@bradshaw.net
 *  License: MIT
 */

!(function (l) {
    if ("undefined" != typeof window) {
        var e,
            m = 0,
            g = !1,
            o = !1,
            v = "message".length,
            I = "[iFrameSizer]",
            x = I.length,
            F = null,
            r = window.requestAnimationFrame,
            h = { max: 1, scroll: 1, bodyScroll: 1, documentElementScroll: 1 },
            M = {},
            i = null,
            w = {
                autoResize: !0,
                bodyBackground: null,
                bodyMargin: null,
                bodyMarginV1: 8,
                bodyPadding: null,
                checkOrigin: !0,
                inPageLinks: !1,
                enablePublicMethods: !0,
                heightCalculationMethod: "bodyOffset",
                id: "iFrameResizer",
                interval: 32,
                log: !1,
                maxHeight: 1 / 0,
                maxWidth: 1 / 0,
                minHeight: 0,
                minWidth: 0,
                resizeFrom: "parent",
                scrolling: !1,
                sizeHeight: !0,
                sizeWidth: !1,
                warningTimeout: 5e3,
                tolerance: 0,
                widthCalculationMethod: "scroll",
                onClose: function () {
                    return !0;
                },
                onClosed: function () {},
                onInit: function () {},
                onMessage: function () {
                    E("onMessage function not defined");
                },
                onResized: function () {},
                onScroll: function () {
                    return !0;
                }
            },
            k = {};
        window.jQuery &&
            ((e = window.jQuery).fn
                ? e.fn.iFrameResize ||
                  (e.fn.iFrameResize = function (i) {
                      return this.filter("iframe")
                          .each(function (e, n) {
                              d(n, i);
                          })
                          .end();
                  })
                : T("", "Unable to bind to jQuery, it is not fully loaded.")),
            "function" == typeof define && define.amd
                ? define([], q)
                : "object" == typeof module &&
                  "object" == typeof module.exports &&
                  (module.exports = q()),
            (window.iFrameResize = window.iFrameResize || q());
    }
    function p() {
        return (
            window.MutationObserver || window.WebKitMutationObserver || window.MozMutationObserver
        );
    }
    function z(e, n, i) {
        e.addEventListener(n, i, !1);
    }
    function O(e, n, i) {
        e.removeEventListener(n, i, !1);
    }
    function a(e) {
        return (
            I +
            "[" +
            (function (e) {
                var n = "Host page: " + e;
                return (
                    window.top !== window.self &&
                        (n =
                            window.parentIFrame && window.parentIFrame.getId
                                ? window.parentIFrame.getId() + ": " + e
                                : "Nested host page: " + e),
                    n
                );
            })(e) +
            "]"
        );
    }
    function t(e) {
        return M[e] ? M[e].log : g;
    }
    function R(e, n) {
        s("log", e, n, t(e));
    }
    function T(e, n) {
        s("info", e, n, t(e));
    }
    function E(e, n) {
        s("warn", e, n, !0);
    }
    function s(e, n, i, t) {
        !0 === t && "object" == typeof window.console && console[e](a(n), i);
    }
    function n(n) {
        function e() {
            i("Height"),
                i("Width"),
                A(
                    function () {
                        P(b), S(y), d("onResized", b);
                    },
                    b,
                    "init"
                );
        }
        function i(e) {
            var n = Number(M[y]["max" + e]),
                i = Number(M[y]["min" + e]),
                t = e.toLowerCase(),
                o = Number(b[t]);
            R(y, "Checking " + t + " is in range " + i + "-" + n),
                o < i && ((o = i), R(y, "Set " + t + " to min value")),
                n < o && ((o = n), R(y, "Set " + t + " to max value")),
                (b[t] = "" + o);
        }
        function t(e) {
            return p.substr(p.indexOf(":") + v + e);
        }
        function a(e, n) {
            !(function (e, n, i) {
                k[i] ||
                    (k[i] = setTimeout(function () {
                        (k[i] = null), e();
                    }, n));
            })(
                function () {
                    B(
                        "Send Page Info",
                        "pageInfo:" +
                            (function () {
                                var e = document.body.getBoundingClientRect(),
                                    n = b.iframe.getBoundingClientRect();
                                return JSON.stringify({
                                    iframeHeight: n.height,
                                    iframeWidth: n.width,
                                    clientHeight: Math.max(
                                        document.documentElement.clientHeight,
                                        window.innerHeight || 0
                                    ),
                                    clientWidth: Math.max(
                                        document.documentElement.clientWidth,
                                        window.innerWidth || 0
                                    ),
                                    offsetTop: parseInt(n.top - e.top, 10),
                                    offsetLeft: parseInt(n.left - e.left, 10),
                                    scrollTop: window.pageYOffset,
                                    scrollLeft: window.pageXOffset,
                                    documentHeight: document.documentElement.clientHeight,
                                    documentWidth: document.documentElement.clientWidth,
                                    windowHeight: window.innerHeight,
                                    windowWidth: window.innerWidth
                                });
                            })(),
                        e,
                        n
                    );
                },
                32,
                n
            );
        }
        function r(e) {
            var n = e.getBoundingClientRect();
            return (
                N(y),
                {
                    x: Math.floor(Number(n.left) + Number(F.x)),
                    y: Math.floor(Number(n.top) + Number(F.y))
                }
            );
        }
        function o(e) {
            var n = e ? r(b.iframe) : { x: 0, y: 0 },
                i = { x: Number(b.width) + n.x, y: Number(b.height) + n.y };
            R(y, "Reposition requested from iFrame (offset x:" + n.x + " y:" + n.y + ")"),
                window.top !== window.self
                    ? window.parentIFrame
                        ? window.parentIFrame["scrollTo" + (e ? "Offset" : "")](i.x, i.y)
                        : E(
                              y,
                              "Unable to scroll to requested position, window.parentIFrame not found"
                          )
                    : ((F = i), s(), R(y, "--"));
        }
        function s() {
            !1 !== d("onScroll", F) ? S(y) : H();
        }
        function d(e, n) {
            return W(y, e, n);
        }
        var c,
            u,
            f,
            l,
            m,
            g,
            h,
            w,
            p = n.data,
            b = {},
            y = null;
        "[iFrameResizerChild]Ready" === p
            ? (function () {
                  for (var e in M) B("iFrame requested init", L(e), M[e].iframe, e);
              })()
            : I === ("" + p).substr(0, x) && p.substr(x).split(":")[0] in M
              ? ((m = p.substr(x).split(":")),
                (g = m[1] ? parseInt(m[1], 10) : 0),
                (h = M[m[0]] && M[m[0]].iframe),
                (w = getComputedStyle(h)),
                (b = {
                    iframe: h,
                    id: m[0],
                    height:
                        g +
                        (function (e) {
                            if ("border-box" !== e.boxSizing) return 0;
                            var n = e.paddingTop ? parseInt(e.paddingTop, 10) : 0,
                                i = e.paddingBottom ? parseInt(e.paddingBottom, 10) : 0;
                            return n + i;
                        })(w) +
                        (function (e) {
                            if ("border-box" !== e.boxSizing) return 0;
                            var n = e.borderTopWidth ? parseInt(e.borderTopWidth, 10) : 0,
                                i = e.borderBottomWidth ? parseInt(e.borderBottomWidth, 10) : 0;
                            return n + i;
                        })(w),
                    width: m[2],
                    type: m[3]
                }),
                (y = b.id),
                M[y] && (M[y].loaded = !0),
                (l = b.type in { true: 1, false: 1, undefined: 1 }) &&
                    R(y, "Ignoring init message from meta parent page"),
                !l &&
                    ((f = !0),
                    M[(u = y)] ||
                        ((f = !1), E(b.type + " No settings for " + u + ". Message was: " + p)),
                    f) &&
                    (R(y, "Received: " + p),
                    (c = !0),
                    null === b.iframe && (E(y, "IFrame (" + b.id + ") not found"), (c = !1)),
                    c &&
                        (function () {
                            var e,
                                i = n.origin,
                                t = M[y] && M[y].checkOrigin;
                            if (
                                t &&
                                "" + i != "null" &&
                                !(t.constructor === Array
                                    ? (function () {
                                          var e = 0,
                                              n = !1;
                                          for (
                                              R(
                                                  y,
                                                  "Checking connection is from allowed list of origins: " +
                                                      t
                                              );
                                              e < t.length;
                                              e++
                                          )
                                              if (t[e] === i) {
                                                  n = !0;
                                                  break;
                                              }
                                          return n;
                                      })()
                                    : ((e = M[y] && M[y].remoteHost),
                                      R(y, "Checking connection is from: " + e),
                                      i === e))
                            )
                                throw new Error(
                                    "Unexpected message received from: " +
                                        i +
                                        " for " +
                                        b.iframe.id +
                                        ". Message was: " +
                                        n.data +
                                        ". This error can be disabled by setting the checkOrigin: false option or by providing of array of trusted domains."
                                );
                            return !0;
                        })() &&
                        (function () {
                            switch (
                                (M[y] && M[y].firstRun && M[y] && (M[y].firstRun = !1), b.type)
                            ) {
                                case "close":
                                    C(b.iframe);
                                    break;
                                case "message":
                                    !(function (e) {
                                        R(
                                            y,
                                            "onMessage passed: {iframe: " +
                                                b.iframe.id +
                                                ", message: " +
                                                e +
                                                "}"
                                        ),
                                            d("onMessage", {
                                                iframe: b.iframe,
                                                message: JSON.parse(e)
                                            }),
                                            R(y, "--");
                                    })(t(6));
                                    break;
                                case "autoResize":
                                    M[y].autoResize = JSON.parse(t(9));
                                    break;
                                case "scrollTo":
                                    o(!1);
                                    break;
                                case "scrollToOffset":
                                    o(!0);
                                    break;
                                case "pageInfo":
                                    a(M[y] && M[y].iframe, y),
                                        (function () {
                                            function e(n, i) {
                                                function t() {
                                                    M[r] ? a(M[r].iframe, r) : o();
                                                }
                                                ["scroll", "resize"].forEach(function (e) {
                                                    R(r, n + e + " listener for sendPageInfo"),
                                                        i(window, e, t);
                                                });
                                            }
                                            function o() {
                                                e("Remove ", O);
                                            }
                                            var r = y;
                                            e("Add ", z), M[r] && (M[r].stopPageInfo = o);
                                        })();
                                    break;
                                case "pageInfoStop":
                                    M[y] &&
                                        M[y].stopPageInfo &&
                                        (M[y].stopPageInfo(), delete M[y].stopPageInfo);
                                    break;
                                case "inPageLink":
                                    !(function (e) {
                                        var n,
                                            i = e.split("#")[1] || "",
                                            t = decodeURIComponent(i),
                                            o =
                                                document.getElementById(t) ||
                                                document.getElementsByName(t)[0];
                                        o
                                            ? ((n = r(o)),
                                              R(
                                                  y,
                                                  "Moving to in page link (#" +
                                                      i +
                                                      ") at x: " +
                                                      n.x +
                                                      " y: " +
                                                      n.y
                                              ),
                                              (F = { x: n.x, y: n.y }),
                                              s(),
                                              R(y, "--"))
                                            : window.top !== window.self
                                              ? window.parentIFrame
                                                  ? window.parentIFrame.moveToAnchor(i)
                                                  : R(
                                                        y,
                                                        "In page link #" +
                                                            i +
                                                            " not found and window.parentIFrame not found"
                                                    )
                                              : R(y, "In page link #" + i + " not found");
                                    })(t(9));
                                    break;
                                case "reset":
                                    j(b);
                                    break;
                                case "init":
                                    e(), d("onInit", b.iframe);
                                    break;
                                default:
                                    e();
                            }
                        })()))
              : T(y, "Ignored: " + p);
    }
    function W(e, n, i) {
        var t = null,
            o = null;
        if (M[e]) {
            if ("function" != typeof (t = M[e][n]))
                throw new TypeError(n + " on iFrame[" + e + "] is not a function");
            o = t(i);
        }
        return o;
    }
    function b(e) {
        var n = e.id;
        delete M[n];
    }
    function C(e) {
        var n = e.id;
        if (!1 !== W(n, "onClose", n)) {
            R(n, "Removing iFrame: " + n);
            try {
                e.parentNode && e.parentNode.removeChild(e);
            } catch (e) {
                E(e);
            }
            W(n, "onClosed", n), R(n, "--"), b(e);
        } else R(n, "Close iframe cancelled by onClose event");
    }
    function N(e) {
        null === F &&
            R(
                e,
                "Get page position: " +
                    (F = {
                        x:
                            window.pageXOffset !== l
                                ? window.pageXOffset
                                : document.documentElement.scrollLeft,
                        y:
                            window.pageYOffset !== l
                                ? window.pageYOffset
                                : document.documentElement.scrollTop
                    }).x +
                    "," +
                    F.y
            );
    }
    function S(e) {
        null !== F &&
            (window.scrollTo(F.x, F.y), R(e, "Set page position: " + F.x + "," + F.y), H());
    }
    function H() {
        F = null;
    }
    function j(e) {
        R(e.id, "Size reset requested by " + ("init" === e.type ? "host page" : "iFrame")),
            N(e.id),
            A(
                function () {
                    P(e), B("reset", "reset", e.iframe, e.id);
                },
                e,
                "reset"
            );
    }
    function P(n) {
        function i(e) {
            o ||
                "0" !== n[e] ||
                ((o = !0),
                R(t, "Hidden iFrame detected, creating visibility listener"),
                (function () {
                    function n() {
                        Object.keys(M).forEach(function (e) {
                            !(function (n) {
                                function e(e) {
                                    return "0px" === (M[n] && M[n].iframe.style[e]);
                                }
                                M[n] &&
                                    null !== M[n].iframe.offsetParent &&
                                    (e("height") || e("width")) &&
                                    B("Visibility change", "resize", M[n].iframe, n);
                            })(e);
                        });
                    }
                    function i(e) {
                        R("window", "Mutation observed: " + e[0].target + " " + e[0].type),
                            c(n, 16);
                    }
                    var t = p();
                    t &&
                        (function () {
                            var e = document.querySelector("body");
                            new t(i).observe(e, {
                                attributes: !0,
                                attributeOldValue: !1,
                                characterData: !0,
                                characterDataOldValue: !1,
                                childList: !0,
                                subtree: !0
                            });
                        })();
                })());
        }
        function e(e) {
            !(function (e) {
                n.id
                    ? ((n.iframe.style[e] = n[e] + "px"),
                      R(n.id, "IFrame (" + t + ") " + e + " set to " + n[e] + "px"))
                    : R("undefined", "messageData id not set");
            })(e),
                i(e);
        }
        var t = n.iframe.id;
        M[t] && (M[t].sizeHeight && e("height"), M[t].sizeWidth && e("width"));
    }
    function A(e, n, i) {
        i !== n.type && r && !window.jasmine ? (R(n.id, "Requesting animation frame"), r(e)) : e();
    }
    function B(e, n, i, t, o) {
        var r,
            a = !1;
        (t = t || i.id),
            M[t] &&
                (i && "contentWindow" in i && null !== i.contentWindow
                    ? ((r = M[t] && M[t].targetOrigin),
                      R(
                          t,
                          "[" +
                              e +
                              "] Sending msg to iframe[" +
                              t +
                              "] (" +
                              n +
                              ") targetOrigin: " +
                              r
                      ),
                      i.contentWindow.postMessage(I + n, r))
                    : E(t, "[" + e + "] IFrame(" + t + ") not found"),
                o &&
                    M[t] &&
                    M[t].warningTimeout &&
                    (M[t].msgTimeout = setTimeout(function () {
                        !M[t] ||
                            M[t].loaded ||
                            a ||
                            ((a = !0),
                            E(
                                t,
                                "IFrame has not responded within " +
                                    M[t].warningTimeout / 1e3 +
                                    " seconds. Check iFrameResizer.contentWindow.js has been loaded in iFrame. This message can be ignored if everything is working, or you can set the warningTimeout option to a higher value or zero to suppress this warning."
                            ));
                    }, M[t].warningTimeout)));
    }
    function L(e) {
        return (
            e +
            ":" +
            M[e].bodyMarginV1 +
            ":" +
            M[e].sizeWidth +
            ":" +
            M[e].log +
            ":" +
            M[e].interval +
            ":" +
            M[e].enablePublicMethods +
            ":" +
            M[e].autoResize +
            ":" +
            M[e].bodyMargin +
            ":" +
            M[e].heightCalculationMethod +
            ":" +
            M[e].bodyBackground +
            ":" +
            M[e].bodyPadding +
            ":" +
            M[e].tolerance +
            ":" +
            M[e].inPageLinks +
            ":" +
            M[e].resizeFrom +
            ":" +
            M[e].widthCalculationMethod
        );
    }
    function d(i, e) {
        function n(e) {
            var n = e.split("Callback");
            if (2 === n.length) {
                var i = "on" + n[0].charAt(0).toUpperCase() + n[0].slice(1);
                (this[i] = this[e]),
                    delete this[e],
                    E(
                        c,
                        "Deprecated: '" +
                            e +
                            "' has been renamed '" +
                            i +
                            "'. The old method will be removed in the next major version."
                    );
            }
        }
        var t,
            o,
            r,
            a,
            s,
            d,
            c =
                ("" === (o = i.id) &&
                    ((i.id =
                        ((t = (e && e.id) || w.id + m++),
                        null !== document.getElementById(t) && (t += m++),
                        (o = t))),
                    (g = (e || {}).log),
                    R(o, "Added missing iframe ID: " + o + " (" + i.src + ")")),
                o);
        function u(e) {
            1 / 0 !== M[c][e] &&
                0 !== M[c][e] &&
                ((i.style[e] = M[c][e] + "px"), R(c, "Set " + e + " = " + M[c][e] + "px"));
        }
        function f(e) {
            if (M[c]["min" + e] > M[c]["max" + e])
                throw new Error("Value for min" + e + " can not be greater than max" + e);
        }
        c in M && "iFrameResizer" in i
            ? E(c, "Ignored iFrame, already setup.")
            : ((d = (d = e) || {}),
              (M[c] = {
                  firstRun: !0,
                  iframe: i,
                  remoteHost: i.src && i.src.split("/").slice(0, 3).join("/")
              }),
              (function (e) {
                  if ("object" != typeof e) throw new TypeError("Options is not an object");
              })(d),
              Object.keys(d).forEach(n, d),
              (function (e) {
                  for (var n in w)
                      Object.prototype.hasOwnProperty.call(w, n) &&
                          (M[c][n] = Object.prototype.hasOwnProperty.call(e, n) ? e[n] : w[n]);
              })(d),
              M[c] &&
                  (M[c].targetOrigin =
                      !0 === M[c].checkOrigin
                          ? (function (e) {
                                return "" === e ||
                                    null !== e.match(/^(about:blank|javascript:|file:\/\/)/)
                                    ? "*"
                                    : e;
                            })(M[c].remoteHost)
                          : "*"),
              (function () {
                  switch (
                      (R(
                          c,
                          "IFrame scrolling " +
                              (M[c] && M[c].scrolling ? "enabled" : "disabled") +
                              " for " +
                              c
                      ),
                      (i.style.overflow = !1 === (M[c] && M[c].scrolling) ? "hidden" : "auto"),
                      M[c] && M[c].scrolling)
                  ) {
                      case "omit":
                          break;
                      case !0:
                          i.scrolling = "yes";
                          break;
                      case !1:
                          i.scrolling = "no";
                          break;
                      default:
                          i.scrolling = M[c] ? M[c].scrolling : "no";
                  }
              })(),
              f("Height"),
              f("Width"),
              u("maxHeight"),
              u("minHeight"),
              u("maxWidth"),
              u("minWidth"),
              ("number" != typeof (M[c] && M[c].bodyMargin) && "0" !== (M[c] && M[c].bodyMargin)) ||
                  ((M[c].bodyMarginV1 = M[c].bodyMargin),
                  (M[c].bodyMargin = M[c].bodyMargin + "px")),
              (r = L(c)),
              (s = p()) &&
                  ((a = s),
                  i.parentNode &&
                      new a(function (e) {
                          e.forEach(function (e) {
                              Array.prototype.slice.call(e.removedNodes).forEach(function (e) {
                                  e === i && C(i);
                              });
                          });
                      }).observe(i.parentNode, { childList: !0 })),
              z(i, "load", function () {
                  B("iFrame.onload", r, i, l, !0),
                      (function () {
                          var e = M[c] && M[c].firstRun,
                              n = M[c] && M[c].heightCalculationMethod in h;
                          !e && n && j({ iframe: i, height: 0, width: 0, type: "init" });
                      })();
              }),
              B("init", r, i, l, !0),
              M[c] &&
                  (M[c].iframe.iFrameResizer = {
                      close: C.bind(null, M[c].iframe),
                      removeListeners: b.bind(null, M[c].iframe),
                      resize: B.bind(null, "Window resize", "resize", M[c].iframe),
                      moveToAnchor: function (e) {
                          B("Move to anchor", "moveToAnchor:" + e, M[c].iframe, c);
                      },
                      sendMessage: function (e) {
                          B("Send Message", "message:" + (e = JSON.stringify(e)), M[c].iframe, c);
                      }
                  }));
    }
    function c(e, n) {
        null === i &&
            (i = setTimeout(function () {
                (i = null), e();
            }, n));
    }
    function u() {
        "hidden" !== document.visibilityState &&
            (R("document", "Trigger event: Visiblity change"),
            c(function () {
                f("Tab Visable", "resize");
            }, 16));
    }
    function f(n, i) {
        Object.keys(M).forEach(function (e) {
            !(function (e) {
                return M[e] && "parent" === M[e].resizeFrom && M[e].autoResize && !M[e].firstRun;
            })(e) || B(n, i, M[e].iframe, e);
        });
    }
    function y() {
        z(window, "message", n),
            z(window, "resize", function () {
                !(function (e) {
                    R("window", "Trigger event: " + e),
                        c(function () {
                            f("Window " + e, "resize");
                        }, 16);
                })("resize");
            }),
            z(document, "visibilitychange", u),
            z(document, "-webkit-visibilitychange", u);
    }
    function q() {
        function i(e, n) {
            n &&
                ((function () {
                    if (!n.tagName) throw new TypeError("Object is not a valid DOM element");
                    if ("IFRAME" !== n.tagName.toUpperCase())
                        throw new TypeError("Expected <IFRAME> tag, found <" + n.tagName + ">");
                })(),
                d(n, e),
                t.push(n));
        }
        var t;
        return (
            (function () {
                var e,
                    n = ["moz", "webkit", "o", "ms"];
                for (e = 0; e < n.length && !r; e += 1) r = window[n[e] + "RequestAnimationFrame"];
                r ? (r = r.bind(window)) : R("setup", "RequestAnimationFrame not supported");
            })(),
            y(),
            function (e, n) {
                switch (
                    ((t = []),
                    (function (e) {
                        e &&
                            e.enablePublicMethods &&
                            E(
                                "enablePublicMethods option has been removed, public methods are now always available in the iFrame"
                            );
                    })(e),
                    typeof n)
                ) {
                    case "undefined":
                    case "string":
                        Array.prototype.forEach.call(
                            document.querySelectorAll(n || "iframe"),
                            i.bind(l, e)
                        );
                        break;
                    case "object":
                        i(e, n);
                        break;
                    default:
                        throw new TypeError("Unexpected data type (" + typeof n + ")");
                }
                return t;
            }
        );
    }
})();
//# sourceMappingURL=iframeResizer.map

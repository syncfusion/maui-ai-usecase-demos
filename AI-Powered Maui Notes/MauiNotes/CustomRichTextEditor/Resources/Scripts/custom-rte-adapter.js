// CustomRichTextEditor engine adapter.
// Runs inside the editor WebView alongside the Syncfusion engine.
//
// Uses the engine's own editor instance (window.RichTextEditorInstance.headlessEditor),
// its ProseMirror state (integration.getState()) and view (integration.getView()).
// Node/mark building uses the PUBLIC ProseMirror Schema API (schema.text,
// NodeType.create, MarkType.create) and public transaction methods (tr.replace,
// tr.replaceWith, tr.insertText, tr.setSelection, view.dispatch).
//
// Key design decisions:
//  - Selection capture stores model positions + revision + selected text/html.
//  - Replacement replaces EXACTLY the captured [from,to) range inside ONE
//    transaction (one undo entry), never via full-document HTML search/replace.
//  - AI HTML is sanitized (unsafe tags/attributes removed) then parsed against
//    the editor schema tag map before any node is created.
//  - Stale detection: revision must match AND text at the range must equal the
//    captured selected text.

// ---------- Syncfusion engine compatibility shims ----------
// On Android, the bundled sf-richtexteditor.js is requested from the
// engine WebView via a file:// URL that the platform's CORS policy blocks.
// When the script is blocked, every reference to the engine helper
// loadFontToDocument() throws an Uncaught ReferenceError on the engine's
// hot path. Defining a no-op shim here ensures the engine never throws
// while the underlying asset is being lazily fetched, and matches the
// behaviour of the missing function (the engine retries font loading
// asynchronously on its own).
(function ensureShims() {
    try {
        if (typeof window.loadFontToDocument !== 'function') {
            window.loadFontToDocument = function () {
                // Intentionally empty. The real implementation lives in
                // sf-richtexteditor.js. The shim prevents repeated
                // ReferenceError noise while the engine script (if
                // blocked by CORS) never resolves.
                return;
            };
        }
    } catch (e) { /* engine may be hostile to global writes; ignore */ }
})();

(function () {
    'use strict';

    if (window.__customRte) { return; }

    function editor() {
        var inst = window.RichTextEditorInstance;
        return (inst && inst.headlessEditor) ? inst.headlessEditor : null;
    }

    function state() {
        var e = editor();
        return e ? e.integration.getState() : null;
    }

    function documentFingerprint(doc) {
        try { return JSON.stringify(doc.toJSON()); }
        catch (e) { return null; }
    }

    function view() {
        var e = editor();
        return e ? e.integration.getView() : null;
    }

    function editorHasDomFocus() {
        var v = view();
        return !!(v && v.dom && v.dom.contains(document.activeElement));
    }

    // ---------- revision tracking ----------
    var revision = 0;
    var lastDocSize = -1;
    // The 400ms doc-size poll below bumps the revision on every observed
    // document mutation. Bumping the revision invalidates any frozen
    // selection automatically: the cached revision no longer matches.

    function bump() { revision++; }

    setInterval(function () {
        var s = state();
        if (!s) { return; }
        var size = s.doc.content.size;
        if (lastDocSize !== -1 && size !== lastDocSize) { bump(); }
        lastDocSize = size;
    }, 400);

    // ---------- frozen selection ----------
    // The editor WebView loses focus when a host button (AI toolbar) steals
    // it, which collapses both the DOM selection and (via the engine's
    // selection observer) the ProseMirror model selection. Subsequent calls
    // to captureSelection would then return an empty range, even though the
    // user clearly had a selection a moment ago.
    //
    // To survive that focus gap, we cache the last non-collapsed selection
    // observed while the editor still owned focus. captureSelection returns
    // this frozen value when the live model is collapsed. The freeze is
    // automatically invalidated by:
    //   - revision mismatch (any mutation invalidates it via bump()),
    //   - doc-size mismatch checked in rangeValid() at apply time,
    //   - an explicit freezeSelection(null) call from our mutations.
    var frozenSelection = null;
    var frozenSelectionRevision = -1;

    function freezeSelection(snap) {
        if (!snap) { frozenSelection = null; frozenSelectionRevision = -1; return; }
        if (!snap.selectedText || snap.selectedText.length === 0) { return; }
        var s = state();
        frozenSelection = Object.assign({}, snap, {
            documentFingerprint: snap.documentFingerprint ||
                (s ? documentFingerprint(s.doc) : null)
        });
        frozenSelectionRevision = revision;
    }

    // ---------- HTML escaping / sanitization ----------
    function escapeHtml(t) {
        return String(t)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;')
            .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    var UNSAFE_TAGS = ['script', 'style', 'iframe', 'object', 'embed',
        'link', 'meta', 'base', 'form', 'input', 'button', 'textarea', 'select'];

    function sanitizeToContainer(html) {
        var container = document.createElement('div');
        container.innerHTML = html;

        UNSAFE_TAGS.forEach(function (tag) {
            var matches = container.querySelectorAll(tag);
            for (var i = matches.length - 1; i >= 0; i--) {
                matches[i].parentNode.removeChild(matches[i]);
            }
        });

        var all = container.querySelectorAll('*');
        for (var j = 0; j < all.length; j++) {
            var el = all[j];
            var attrs = Array.prototype.slice.call(el.attributes);
            attrs.forEach(function (a) {
                var lname = a.name.toLowerCase();
                if (lname.indexOf('on') === 0) { el.removeAttribute(a.name); return; }
                if ((lname === 'href' || lname === 'src') && /^\s*javascript:/i.test(a.value)) {
                    el.removeAttribute(a.name); return;
                }
                if (lname === 'class' || lname === 'id' || lname === 'contenteditable' ||
                    lname === 'spellcheck' || lname === 'data-pm-slice') {
                    el.removeAttribute(a.name);
                }
            });
        }
        return container;
    }

    // ---------- color / font helpers ----------
    function normalizeColor(color) {
        try {
            var probe = document.createElement('span');
            probe.style.color = color;
            document.body.appendChild(probe);
            var rgb = probe.style.color;
            document.body.removeChild(probe);
            var m = /rgba?\((\d+)[,\s]+(\d+)[,\s]+(\d+)/.exec(rgb);
            if (m) {
                function h2(v) { return ('0' + parseInt(v, 10).toString(16)).slice(-2); }
                return '#' + h2(m[1]) + h2(m[2]) + h2(m[3]);
            }
        } catch (e) { /* fall through */ }
        return color;
    }

    function normalizeFontSize(fs) {
        var m = /^([\d.]+)\s*(px|pt|em|rem)?$/.exec(String(fs).trim());
        if (!m) { return String(fs); }
        var val = parseFloat(m[1]);
        if (m[2] === 'pt') { val = val * 96 / 72; }
        else if (m[2] === 'em' || m[2] === 'rem') { val = val * 16; }
        if (m[2]) { return Math.round(val) + 'px'; }
        return String(fs);
    }

    // ---------- DOM -> schema nodes ----------
    function isBlockish(el) {
        if (!el || el.nodeType !== 1) { return false; }
        var t = el.tagName ? el.tagName.toLowerCase() : '';
        return ['p', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
            'ul', 'ol', 'pre', 'figure', 'blockquote', 'table'].indexOf(t) !== -1;
    }

    function collectInline(el, schema, marks, out) {
        var children = el.childNodes;
        for (var i = 0; i < children.length; i++) {
            var child = children[i];
            if (child.nodeType === 3) {
                if (child.textContent) { out.push(schema.text(child.textContent, marks)); }
            } else if (child.nodeType === 1) {
                var tag = child.tagName.toLowerCase();
                if (tag === 'br') {
                    out.push(schema.text('\n', marks)); // engine renders newline in code contexts
                } else if (tag === 'img') {
                    var imgType = schema.nodes.image;
                    if (imgType && child.getAttribute('src')) {
                        out.push(imgType.create({
                            src: child.getAttribute('src'),
                            alt: child.getAttribute('alt') || null,
                            title: child.getAttribute('title') || null
                        }));
                    }
                } else if (tag === 'a') {
                    if (schema.marks.link) {
                        var linkMarks = marks.slice();
                        linkMarks.push(schema.marks.link.create({
                            href: child.getAttribute('href') || '#',
                            target: child.getAttribute('target') || '_blank',
                            rel: child.getAttribute('rel') || 'noopener noreferrer',
                            title: child.getAttribute('title') || null
                        }));
                        collectInline(child, schema, linkMarks, out);
                    } else {
                        collectInline(child, schema, marks, out);
                    }
                } else {
                    var markType = markTypeFor(tag, child, schema);
                    if (markType) {
                        collectInline(child, schema, marks.concat([markType]), out);
                    } else {
                        collectInline(child, schema, marks, out);
                    }
                }
            }
        }
        return out;
    }

    function markTypeFor(tag, el, schema) {
        switch (tag) {
            case 'b': case 'strong':
                return schema.marks.bold ? schema.marks.bold.create() : null;
            case 'i': case 'em':
                return schema.marks.italic ? schema.marks.italic.create() : null;
            case 'u':
                return schema.marks.underline ? schema.marks.underline.create() : null;
            case 's': case 'del': case 'strike':
                return schema.marks.strikethrough ? schema.marks.strikethrough.create() : null;
            case 'code':
                return schema.marks.code ? schema.marks.code.create() : null;
            case 'sup':
                return schema.marks.superscript ? schema.marks.superscript.create() : null;
            case 'sub':
                return schema.marks.subscript ? schema.marks.subscript.create() : null;
            default:
                if ((tag === 'span' || tag === 'font') && schema.marks.textStyle) {
                    var color = el.getAttribute('color');
                    var style = el.getAttribute('style') || '';
                    var m = style ? /(?:^|;)\s*color\s*:\s*([^;]+)/i.exec(style) : null;
                    var fs = /font-size\s*:\s*([^;]+)/i.exec(style);
                    var ff = /font-family\s*:\s*([^;]+)/i.exec(style);
                    var attrs = {};
                    if (color) { attrs.color = normalizeColor(color); }
                    else if (m) { attrs.color = normalizeColor(m[1].trim()); }
                    if (fs) { attrs.fontSize = normalizeFontSize(fs[1].trim()); }
                    if (ff) { attrs.fontFamily = ff[1].trim(); }
                    if (attrs.color || attrs.fontSize || attrs.fontFamily) {
                        return schema.marks.textStyle.create(attrs);
                    }
                }
                return null;
        }
    }

    function paragraphFrom(el, schema, explicitContent) {
        var content = explicitContent !== undefined ? explicitContent :
            collectInline(el, schema, [], []);
        var para = schema.nodes.paragraph;
        if (content && content.length) {
            return para.create(null, content);
        }
        return para.create();
    }

    function codeBlockFrom(pre, schema) {
        var codeType = schema.nodes.codeBlock;
        if (!codeType) { return paragraphFrom(pre, schema); }
        var languageSel = pre.querySelector('select');
        var language = languageSel ? (languageSel.value || 'plaintext') :
            (pre.getAttribute('data-language') || 'plaintext');
        var text = (pre.innerText || pre.textContent || '').replace(/\u00a0/g, ' ');
        if (text) {
            return codeType.create({ language: language }, schema.text(text));
        }
        return codeType.create({ language: language });
    }

    function tableFrom(tableEl, schema) {
        var tableType = schema.nodes.table;
        var rowType = schema.nodes.tableRow;
        var cellType = schema.nodes.tableCell;
        var headerType = schema.nodes.tableHeader;
        if (!tableType || !rowType || !cellType) { return null; }

        var rows = [];
        var trs = tableEl.querySelectorAll('tr');
        for (var r = 0; r < trs.length; r++) {
            var cells = [];
            var cellEls = trs[r].children;
            for (var c = 0; c < cellEls.length; c++) {
                var cellEl = cellEls[c];
                var isHeader = cellEl.tagName.toLowerCase() === 'th';
                var type = (isHeader && headerType) ? headerType : cellType;
                var paras = [];
                for (var k = 0; k < cellEl.children.length; k++) {
                    var inner = cellEl.children[k];
                    var innerTag = inner.tagName.toLowerCase();
                    if (innerTag === 'ul' || innerTag === 'ol') {
                        // flatten lists inside cells to paragraphs (schema permit)
                        var lis = inner.querySelectorAll('li');
                        for (var li = 0; li < lis.length; li++) {
                            paras.push(paragraphFrom(lis[li], schema));
                        }
                    } else {
                        paras.push(paragraphFrom(inner, schema));
                    }
                }
                if (!paras.length) {
                    var direct = cellEl.textContent || '';
                    paras.push(direct.trim() ?
                        paragraphFrom(cellEl, schema) :
                        schema.nodes.paragraph.create());
                }
                cells.push(type.create({
                    colspan: parseInt(cellEl.getAttribute('colspan') || '1', 10) || 1,
                    rowspan: parseInt(cellEl.getAttribute('rowspan') || '1', 10) || 1
                }, paras));
            }
            if (cells.length) { rows.push(rowType.create(null, cells)); }
        }
        return rows.length ? tableType.create(null, rows) : null;
    }

    function listBlocks(ul, schema, out) {
        var listTag = ul.tagName.toLowerCase();
        var listName = listTag === 'ul' ? 'bulletList' : 'orderedList';
        var listType = schema.nodes[listName];
        var itemType = schema.nodes.listItem;
        if (!listType || !itemType) {
            // flatten: each li becomes a paragraph
            var lis = ul.querySelectorAll('li');
            for (var i = 0; i < lis.length; i++) { out.push(paragraphFrom(lis[i], schema)); }
            return;
        }

        var items = [];
        for (var j = 0; j < ul.children.length; j++) {
            var li = ul.children[j];
            if (li.tagName.toLowerCase() !== 'li') { continue; }

            // The engine's listItem schema is:
            //   G.sequence(G.choice(paragraph, heading), G.block().zeroOrMore())
            // The first child MUST be a paragraph or heading; additional block
            // nodes (nested lists) follow. The AI formatter emits
            // <li><p>text</p></li> for bullet/ordered lists, so the common
            // case is a single <p> child. Preserve that exactly so the
            // listItem structure matches what the engine expects.
            var items_firstChild = null;
            var nested = [];
            for (var k = 0; k < li.childNodes.length; k++) {
                var ch = li.childNodes[k];
                if (ch.nodeType === 1) {
                    var childTag = ch.tagName ? ch.tagName.toLowerCase() : '';
                    if (childTag === 'p' || childTag === 'h1' || childTag === 'h2' ||
                        childTag === 'h3' || childTag === 'h4' || childTag === 'h5' || childTag === 'h6') {
                        if (items_firstChild === null) { items_firstChild = ch; }
                        else { nested.push(ch); }
                    } else if (isBlockish(ch)) {
                        nested.push(ch);
                    } else {
                        // Inline element: collect with the leading inline
                        // content; treat as paragraph text.
                        if (items_firstChild === null) { items_firstChild = ch; }
                    }
                } else if (ch.nodeType === 3 && ch.textContent && ch.textContent.length) {
                    // Leading text node: attach to the first inline holder
                    // (or directly to the first child if it is a span/holder).
                    if (items_firstChild === null) {
                        var inlineHolder = document.createElement('span');
                        inlineHolder.appendChild(ch.cloneNode(true));
                        items_firstChild = inlineHolder;
                    }
                }
            }

            var para;
            if (items_firstChild) {
                var firstTag = items_firstChild.tagName ? items_firstChild.tagName.toLowerCase() : '';
                if (firstTag === 'p') {
                    para = paragraphFrom(items_firstChild, schema);
                } else if (firstTag === 'h1' || firstTag === 'h2' || firstTag === 'h3' ||
                           firstTag === 'h4' || firstTag === 'h5' || firstTag === 'h6') {
                    var level = parseInt(firstTag.charAt(1), 10);
                    var inline = collectInline(items_firstChild, schema, [], []);
                    if (schema.nodes.heading) {
                        para = schema.nodes.heading.create({ level: level },
                            inline.length ? inline : [schema.text('')]);
                    } else {
                        para = paragraphFrom(items_firstChild, schema);
                    }
                } else {
                    para = paragraphFrom(items_firstChild, schema);
                }
            } else {
                para = schema.nodes.paragraph.create();
            }

            var content = [para];
            for (var n = 0; n < nested.length; n++) {
                var nestedBlocks = [];
                parseIntoBlocks(nested[n], schema, nestedBlocks);
                for (var b = 0; b < nestedBlocks.length; b++) { content.push(nestedBlocks[b]); }
            }
            items.push(itemType.create(null, content));
        }
        if (items.length) { out.push(listType.create(null, items)); }
    }

    function parseIntoBlocks(el, schema, out) {
        if (!el || el.nodeType !== 1) { return; }
        var tag = el.tagName.toLowerCase();

        switch (tag) {
            case 'ul': case 'ol':
                listBlocks(el, schema, out);
                return;
            case 'pre':
                out.push(codeBlockFrom(el, schema));
                return;
            case 'table':
                var t = tableFrom(el, schema);
                if (t) { out.push(t); }
                return;
            case 'figure': case 'blockquote':
                // engine schema has no blockquote/figure: unwrap children as blocks
                var cb = [];
                for (var i = 0; i < el.childNodes.length; i++) {
                    var c = el.childNodes[i];
                    if (c.nodeType === 1) { parseIntoBlocks(c, schema, cb); }
                }
                if (cb.length) {
                    for (var j = 0; j < cb.length; j++) { out.push(cb[j]); }
                } else {
                    out.push(paragraphFrom(el, schema));
                }
                return;
            case 'h1': case 'h2': case 'h3': case 'h4': case 'h5': case 'h6':
                var level = parseInt(tag.charAt(1), 10);
                if (schema.nodes.heading) {
                    var inline = collectInline(el, schema, [], []);
                    out.push(schema.nodes.heading.create({ level: level },
                        inline.length ? inline : [schema.text('')]));
                    return;
                }
                out.push(paragraphFrom(el, schema));
                return;
            case 'hr':
                // engine has no horizontalRule node: skip
                return;
            case 'div':
                // div: if it contains block children, unwrap; else paragraph
                var seenBlock = false;
                for (var d = 0; d < el.childNodes.length; d++) {
                    if (isBlockish(el.childNodes[d])) { seenBlock = true; break; }
                }
                if (seenBlock) {
                    for (var e = 0; e < el.children.length; e++) {
                        parseIntoBlocks(el.children[e], schema, out);
                    }
                } else {
                    out.push(paragraphFrom(el, schema));
                }
                return;
            default:
                out.push(paragraphFrom(el, schema));
                return;
        }
    }

    function parseHtmlFragment(html, schema) {
        var container = sanitizeToContainer(html);
        var blocks = [];
        var kids = container.childNodes;
        var meaningful = [];
        for (var i = 0; i < kids.length; i++) {
            var n = kids[i];
            if (n.nodeType === 3) {
                if (n.textContent.trim()) { meaningful.push(n); }
            } else if (n.nodeType === 1) {
                meaningful.push(n);
            }
        }
        if (!meaningful.length) { return blocks; }

        // Detect pure-inline fragment (AI result like "<b>Mango</b>" or plain "Mango")
        var inlineOnly = true;
        for (var q = 0; q < meaningful.length; q++) {
            if (meaningful[q].nodeType === 1 && isBlockish(meaningful[q])) {
                inlineOnly = false; break;
            }
        }

        if (inlineOnly) {
            // wrap the whole fragment into ONE paragraph so AI copy stays together
            var wrapper = document.createElement('p');
            for (var w = 0; w < meaningful.length; w++) {
                wrapper.appendChild(meaningful[w].cloneNode(true));
            }
            blocks.push(paragraphFrom(wrapper, schema));
        } else {
            for (var z = 0; z < meaningful.length; z++) {
                var node = meaningful[z];
                if (node.nodeType === 3) {
                    var pw = document.createElement('p');
                    pw.appendChild(node.cloneNode(true));
                    blocks.push(paragraphFrom(pw, schema));
                } else {
                    parseIntoBlocks(node, schema, blocks);
                }
            }
        }
        return blocks;
    }

    function plainTextToBlocks(text, schema, marks) {
        var lines = String(text).replace(/\r\n?/g, '\n').split('\n');
        var blocks = [];
        for (var i = 0; i < lines.length; i++) {
            blocks.push(lines[i].length ?
                schema.nodes.paragraph.create(null, schema.text(lines[i], marks || [])) :
                schema.nodes.paragraph.create());
        }
        if (!blocks.length) { blocks.push(schema.nodes.paragraph.create()); }
        return blocks;
    }

    // Preserve formatting that applies consistently to the replaced text.
    // Intersecting marks avoids applying bold/color/link styles from only one
    // part of a selection when the selected range contains mixed formatting.
    function marksInRange(doc, from, to) {
        var commonMarks = null;
        doc.nodesBetween(from, to, function (node) {
            if (!node.isText) { return; }
            if (commonMarks === null) {
                commonMarks = node.marks.slice();
                return;
            }
            commonMarks = commonMarks.filter(function (mark) {
                return node.marks.some(function (candidate) { return mark.eq(candidate); });
            });
        });

        return commonMarks === null ? doc.resolve(from).marks().slice() : commonMarks;
    }

    // Marks that would apply to text typed at the caret right now: stored
    // marks (toolbar toggles awaiting the next keystroke) first, otherwise
    // the marks of the character before the caret.
    function caretMarks(s) {
        try {
            if (s.storedMarks && s.storedMarks.length) { return s.storedMarks.slice(); }
            return s.doc.resolve(s.selection.from).marks().slice();
        } catch (e) { return []; }
    }

    function addMarksRange(tr, from, to, marks) {
        (marks || []).forEach(function (m) { tr.addMark(from, to, m); });
        return tr;
    }

    // Inserts plain text while PRESERVING the surrounding block structure:
    //  - The first line replaces the captured range inline, INSIDE the
    //    existing parent block — heading level, list context, alignment and
    //    block attributes are kept because no new paragraph is created.
    //  - Inline marks are re-applied over the replaced range so bold/color/
    //    links of the original selection survive.
    //  - Each following line continues as a sibling block of the SAME type:
    //    a second heading line stays a heading, list-item lines become new
    //    list items (never paragraphs nested inside the current item), and
    //    code blocks receive the whole text verbatim (newlines included,
    //    no marks — code styling comes from the block).
    // This makes AI-inserted content structurally identical to manually
    // typed content, so subsequent typing and save/reload keep formatting.
    function structuredTextInsert(tr, schema, $from, parent, from, to, text, marks) {
        if (parent.type.spec.code !== undefined) {
            // Code block: whole text in ONE insert — code content may span
            // newlines, and marks must never apply inside code.
            return tr.insertText(String(text), from, to);
        }

        var lines = String(text).replace(/\r\n?/g, '\n').split('\n');
        var useMarks = marks || [];
        var first = lines[0] || '';
        tr = tr.insertText(first, from, to);
        if (first) { addMarksRange(tr, from, from + first.length, useMarks); }
        var pos = from + first.length;
        for (var i = 1; i < lines.length; i++) {
            var inline = lines[i] ? schema.text(lines[i], useMarks) : null;
            var follow = followUpBlock(schema, $from, parent, inline);
            tr = tr.insert(pos, follow);
            pos += follow.nodeSize;
        }
        return tr;
    }

    // Builds the sibling block for a continuation line: the SAME textblock
    // type as the parent (a second heading line stays a heading, a paragraph
    // line stays a paragraph...). When the parent textblock is itself a list
    // item (this engine stores <li> text directly in the listItem), the
    // continuation is a new listItem so AI list lines become real new list
    // items instead of paragraphs nested inside the current item.
    function followUpBlock(schema, $from, parent, inline) {
        var follow = parent.type.create(parent.attrs, inline);
        if (parent.type.name === 'listItem') {
            return follow;   // new sibling list item, same attrs
        }
        if ($from.depth >= 2) {
            var container = $from.node($from.depth - 1);
            if (container && container.type.name === 'listItem' &&
                container.type.contentMatch.matchType(parent.type) !== null) {
                return container.type.create(container.attrs,
                    parent.type.create(parent.attrs, inline));
            }
        }
        return follow;
    }

    // Replaces [from, to] with a SEPARATE block per parsed block node —
    // unlike tr.replace(sliceFrom(...)), which merges the inline content of
    // every block into one fragment and collapses multi-line results into
    // a single text run. Applies only in the multi-block fallback path.
    function insertBlocks(tr, schema, from, to, blocks) {
        var pos = from;
        tr = tr.delete(from, to);
        for (var i = 0; i < blocks.length; i++) {
            tr = tr.insert(pos, blocks[i]);
            pos += blocks[i].nodeSize;
        }
        return tr;
    }

    // Resolves the textblock parent shared by [from, to] together with its
    // resolved position, or null when the range spans multiple blocks / sits
    // outside a textblock (block-level selection, caret between blocks,
    // table cell boundary...). Null means the caller must fall back to
    // whole-block replacement.
    function sharedTextblockParent(doc, from, to) {
        try {
            var $from = doc.resolve(from), $to = doc.resolve(to);
            if ($from.parent.isTextblock && $from.parent === $to.parent) {
                return { parent: $from.parent, from: $from };
            }
        } catch (e) { /* fall through */ }
        return null;
    }

    // ---------- schema name -> HTML serialization (for SelectedHtml) ----------
    function serializeFragment(fragment, schema) {
        var html = '';
        fragment.forEach(function (node) {
            html += serializeNode(node, schema, []);
        });
        return html;
    }

    function serializeNode(node, schema) {
        if (node.isText) {
            var open = '', close = '';
            node.marks.forEach(function (m) { open += markOpen(m); });
            for (var i = node.marks.length - 1; i >= 0; i--) { close += markClose(node.marks[i]); }
            return open + escapeHtml(node.text) + close;
        }

        var name = node.type.name;
        var attrs = node.attrs || {};
        function attrStr() {
            var s = '';
            for (var k in attrs) {
                if (Object.prototype.hasOwnProperty.call(attrs, k) &&
                    attrs[k] !== null && attrs[k] !== undefined) {
                    s += ' ' + k + '="' + escapeHtml(String(attrs[k])) + '"';
                }
            }
            return s;
        }

        switch (name) {
            case 'paragraph': return '<p>' + innerHtml(node, schema) + '</p>';
            case 'heading': return '<h' + (attrs.level || 1) + attrStr() + '>' + innerHtml(node, schema) + '</h' + (attrs.level || 1) + '>';
            case 'codeBlock':
                var txt = '';
                node.forEach(function (c) { if (c.isText) { txt += c.text; } });
                return '<pre>' + escapeHtml(txt) + '</pre>';
            case 'image':
                return '<img src="' + escapeHtml(attrs.src || '') + '"' +
                    (attrs.alt ? ' alt="' + escapeHtml(attrs.alt) + '"' : '') +
                    (attrs.title ? ' title="' + escapeHtml(attrs.title) + '"' : '') + '>';
            case 'bulletList': return '<ul>' + innerHtml(node, schema) + '</ul>';
            case 'orderedList': return '<ol>' + innerHtml(node, schema) + '</ol>';
            case 'listItem': return '<li>' + innerHtml(node, schema) + '</li>';
            case 'table': return '<table>' + innerHtml(node, schema) + '</table>';
            case 'tableRow': return '<tr>' + innerHtml(node, schema) + '</tr>';
            case 'tableHeader': return '<th' + attrStr() + '>' + innerHtml(node, schema) + '</th>';
            case 'tableCell': return '<td' + attrStr() + '>' + innerHtml(node, schema) + '</td>';
            default: return innerHtml(node, schema);
        }
    }

    function innerHtml(node, schema) {
        var out = '';
        node.forEach(function (child) { out += serializeNode(child, schema); });
        return out;
    }

    function markOpen(m) {
        switch (m.type.name) {
            case 'bold': return '<b>';
            case 'italic': return '<i>';
            case 'underline': return '<u>';
            case 'strikethrough': return '<s>';
            case 'code': return '<code>';
            case 'superscript': return '<sup>';
            case 'subscript': return '<sub>';
            case 'link':
                return '<a href="' + escapeHtml(m.attrs.href || '#') + '"' +
                    (m.attrs.target ? ' target="' + escapeHtml(m.attrs.target) + '"' : '') +
                    (m.attrs.title ? ' title="' + escapeHtml(m.attrs.title) + '"' : '') + '>';
            case 'textStyle':
                var style = '';
                if (m.attrs.color) { style += 'color:' + m.attrs.color + ';'; }
                if (m.attrs.fontSize) { style += 'font-size:' + m.attrs.fontSize + ';'; }
                if (m.attrs.fontFamily) { style += 'font-family:' + m.attrs.fontFamily + ';'; }
                return style ? '<span style="' + escapeHtml(style) + '">' : '';
            default: return '';
        }
    }

    function markClose(m) {
        switch (m.type.name) {
            case 'bold': return '</b>';
            case 'italic': return '</i>';
            case 'underline': return '</u>';
            case 'strikethrough': return '</s>';
            case 'code': return '</code>';
            case 'superscript': return '</sup>';
            case 'subscript': return '</sub>';
            case 'link': return '</a>';
            case 'textStyle': return '</span>';
            default: return '';
        }
    }

    // ---------- selection capture ----------
    function captureRange(s, from, to) {
        var selectedText = '';
        try { selectedText = s.doc.textBetween(from, to, '\n', ''); } catch (e) { selectedText = ''; }
        var selectedHtml = '';
        try {
            var slice = s.doc.slice(from, to);
            selectedHtml = serializeFragment(slice.content, s.schema);
        } catch (e2) { selectedHtml = ''; }

        lastDocSize = s.doc.content.size;
        return {
            revision: revision,
            from: from,
            to: to,
            selectedText: selectedText,
            selectedHtml: selectedHtml,
            documentFingerprint: documentFingerprint(s.doc)
        };
    }

    function captureSelection() {
        var s = state();
        if (!s) { return null; }

        // The browser selection is the user's visible selection. Read it before
        // the model selection because the editor's selection observer can lag
        // behind a fast toolbar click.
        var v = view();
        var domSelection = window.getSelection();
        if (v && v.dom && domSelection && !domSelection.isCollapsed &&
            domSelection.rangeCount > 0 &&
            v.dom.contains(domSelection.anchorNode) &&
            v.dom.contains(domSelection.focusNode)) {
            try {
                var anchor = v.posAtDOM(domSelection.anchorNode, domSelection.anchorOffset);
                var focus = v.posAtDOM(domSelection.focusNode, domSelection.focusOffset);
                var from = Math.min(anchor, focus);
                var to = Math.max(anchor, focus);
                if (from < to) {
                    var mapped = captureRange(s, from, to);
                    if (mapped.selectedText && mapped.selectedText.length > 0) {
                        freezeSelection(mapped);
                        return mapped;
                    }
                }
            } catch (e) {
                // Fall back to the editor's model selection when the DOM range
                // cannot be mapped safely.
            }
        }

        // Use the model selection only after checking the visible browser range.
        if (s.selection && s.selection.from < s.selection.to) {
            var live = captureRange(s, s.selection.from, s.selection.to);
            if (live.selectedText && live.selectedText.length > 0) {
                freezeSelection(live);
                return live;
            }
        }

        // Live selection is collapsed AND the browser selection is gone (the
        // editor WebView lost focus to a host control such as the AI toolbar
        // button). Serve the last non-collapsed selection we observed so the
        // host can still identify the content the user intended to act on.
        // The apply path uses rangeValid() to refuse a stale or moved-range
        // freeze, so it is safe to return this even when uncertainty exists.
        if (frozenSelection && frozenSelectionRevision === revision &&
            frozenSelection.documentFingerprint === documentFingerprint(s.doc) &&
            frozenSelection.to <= s.doc.content.size) {
            var size = s.doc.content.size;
            if (frozenSelection.from >= 0 && frozenSelection.to > frozenSelection.from &&
                frozenSelection.to <= size) {
                // Re-derive the snapshot against the current document so the
                // returned selectedText/selectedHtml match the live content
                // at the stored model positions. The frozenSelection is the
                // authoritative range; the surrounding text may have shifted
                // if the user edited elsewhere, but the text AT that exact
                // range is still the value they meant to act on.
                return captureRange(s, frozenSelection.from, frozenSelection.to);
            }
        }

        return captureRange(s, s.selection.from, s.selection.to);
    }

    function captureDocument() {
        var s = state();
        if (!s) { return null; }
        return captureRange(s, 0, s.doc.content.size);
    }

    function getStoredSelection() {
        var s = state();
        if (!s || !frozenSelection ||
            frozenSelectionRevision !== revision ||
            frozenSelection.documentFingerprint !== documentFingerprint(s.doc)) {
            return null;
        }

        var from = frozenSelection.from;
        var to = frozenSelection.to;
        if (from < 0 || to <= from || to > s.doc.content.size) {
            return null;
        }

        var currentText = '';
        try { currentText = s.doc.textBetween(from, to, '\n', ''); }
        catch (e) { return null; }
        if (currentText !== frozenSelection.selectedText) {
            return null;
        }

        return captureRange(s, from, to);
    }

    // ---------- stale detection ----------
    function rangeValid(s, captured) {
        var size = s.doc.content.size;
        var from = captured.from, to = captured.to;
        if (from < 0 || to < from || to > size) { return false; }
        if (captured.revision !== revision) { return false; }
        if (!captured.documentFingerprint ||
            captured.documentFingerprint !== documentFingerprint(s.doc)) { return false; }
        // verify content at range is unchanged
        var currentText = '';
        try { currentText = s.doc.textBetween(from, to, '\n', ''); } catch (e) { return false; }
        return currentText === captured.selectedText;
    }

    // ---------- Slice construction ----------
    function sliceFrom(blocks, schema) {
        // Reuse ProseMirror Slice class by probing with doc.slice(0, 0)
        var s = state();
        if (!s) { return null; }
        var probe = s.doc.slice(0, 0);
        var SliceCtor = probe.constructor;

        if (blocks.length === 1) {
            // Single block: open it fully so it merges at the caret
            var single = blocks[0];
            if (single.content && single.content.size > 0) {
                return new SliceCtor(single.content, 1, 1);
            }
            return new SliceCtor(single.content, 0, 0);
        }

        // Multiple blocks: build a Fragment via a paragraph container then extract
        var fragArray = [];
        blocks.forEach(function (b) {
            if (b.type.name === 'paragraph') {
                b.forEach(function (c) { fragArray.push(c); });
            } else {
                fragArray.push(b);
            }
        });
        var result = null;
        fragmentFromArray(fragArray, schema, function (frag) {
            result = new SliceCtor(frag, 0, 0);
        });
        return result;
    }

    function fragmentFromArray(array, schema, cb) {
        // Fragment class reachable via schema.text('x','').content.constructor
        try {
            var t = schema.text('x');
            var FragCtor = t.content.constructor;
            cb(new FragCtor(array));
        } catch (e) {
            cb(null);
        }
    }

    // ---------- replace ----------
    function replaceRange(captured, content, contentType) {
        var s = state();
        if (!s) { return { ok: false, error: 'editor state unavailable' }; }
        if (!rangeValid(s, captured)) { return { ok: false, stale: true }; }

        try {
            var schema = s.schema;
            var v = view();
            if (!v) { return { ok: false, error: 'editor view unavailable' }; }

            var from = captured.from, to = captured.to;
            var tr = s.tr;

            if (contentType === 'PlainText') {
                // Formatting preservation: replace the captured range with the
                // AI text while keeping the surrounding mark and block
                // structure. When the range sits inside ONE textblock (the
                // common user selection case) the replacement happens inline —
                // no artificial paragraphs are created, the heading/list/code
                // context remains, and inherited marks are re-applied. Only
                // when the range spans multiple blocks does this fall back to
                // fresh paragraph blocks (with the selection's common marks).
                var shared = sharedTextblockParent(s.doc, from, to);
                if (shared) {
                    var inherited = to > from
                        ? marksInRange(s.doc, from, to)
                        : s.doc.resolve(from).marks().slice();
                    tr = structuredTextInsert(tr, schema, shared.from, shared.parent, from, to, content, inherited);
                } else {
                    var fallbackMarks = to > from
                        ? marksInRange(s.doc, from, to)
                        : s.doc.resolve(from).marks().slice();
                    var blocks = plainTextToBlocks(content, schema, fallbackMarks);
                    if (!blocks.length) {
                        return { ok: false, error: 'parsed content is empty' };
                    }
                    // Multi-block ranges: insertEach keeps one block per line
                    // (no line collapse), each built with the common marks.
                    tr = insertBlocks(tr, schema, from, to, blocks);
                    ensureDocNonEmpty(tr, schema);
                }
            } else if (contentType === 'Html') {
                var blocks = parseHtmlFragment(content, schema);
                if (!blocks.length) {
                    return { ok: false, error: 'parsed content is empty' };
                }
                tr = tr.replace(from, to, sliceFrom(blocks, schema) || undefined);
                ensureDocNonEmpty(tr, schema);
                var addedTrailingParagraph = ensureTrailingParagraphAfterTable(tr, schema, blocks);
            } else {
                return { ok: false, error: 'unsupported content type: ' + contentType };
            }

            // Caret after inserted content
            var appliedFrom = tr.mapping.map(from, -1);
            var appliedTo = tr.mapping.map(to, 1);
            var appliedText = tr.doc.textBetween(appliedFrom, appliedTo, '\n', '');
            var appliedFingerprint = documentFingerprint(tr.doc);
            var caret = contentType === 'Html' && addedTrailingParagraph
                ? tr.doc.content.size - 1
                : tr.mapping.map(Math.max(from, captured.to));
            placeCaret(tr, caret, addedTrailingParagraph);

            v.dispatch(tr);
            bump();
            lastDocSize = tr.doc.content.size;
            // The document changed in response to a request from the host.
            // Any frozen selection that targeted the replaced range is no
            // longer valid; the host will re-capture against the new model.
            freezeSelection(null);
            return {
                ok: true,
                revision: revision,
                appliedFrom: appliedFrom,
                appliedTo: appliedTo,
                appliedText: appliedText,
                documentFingerprint: appliedFingerprint
            };
        } catch (err) {
            return { ok: false, error: String(err && err.message ? err.message : err) };
        }
    }

    function replaceDocument(content, contentType) {
        var s = state();
        if (!s) { return { ok: false, error: 'editor state unavailable' }; }
        var captured = captureRange(s, 0, s.doc.content.size);
        return replaceRange(captured, content, contentType);
    }

    function blocksToText(blocks) {
        var out = '';
        blocks.forEach(function (b) {
            b.forEach(function (c) {
                if (c.isText) { out += c.text; }
            });
        });
        return out;
    }

    function ensureDocNonEmpty(tr, schema) {
        if (tr.doc.content.size === 0 && schema.nodes.paragraph) {
            // insert empty paragraph to keep the doc valid
            tr.insert(0, schema.nodes.paragraph.create());
        }
    }

    function ensureTrailingParagraphAfterTable(tr, schema, insertedBlocks) {
        var lastNode = tr.doc.lastChild;
        var lastInsertedBlock = insertedBlocks && insertedBlocks[insertedBlocks.length - 1];
        if (!lastInsertedBlock || lastInsertedBlock.type.name !== 'table' ||
            !lastNode || lastNode.type.name !== 'table') {
            return false;
        }

        var paragraphType = schema.nodes.paragraph;
        if (!paragraphType) {
            throw new Error('Cannot add an editable paragraph after the final table: paragraph node unavailable');
        }

        tr.insert(tr.doc.content.size, paragraphType.create());
        return true;
    }

    function placeCaret(tr, pos, required) {
        var s = state();
        if (!s) {
            if (required) { throw new Error('Cannot place the caret: editor selection unavailable'); }
            return;
        }

        var selCtor = s.selection.constructor;
        var clamped = Math.max(0, Math.min(pos, tr.doc.content.size));
        try {
            tr = tr.setSelection(selCtor.create(tr.doc, clamped));
        } catch (createError) {
            try {
                tr = tr.setSelection(selCtor.near(tr.doc.resolve(clamped), 1));
            } catch (nearError) {
                if (!required) { return; }
                throw new Error(
                    'Cannot place the caret at the inserted content: ' +
                    String(nearError && nearError.message ? nearError.message :
                        createError && createError.message ? createError.message : nearError));
            }
        }
    }

    // ---------- insert at caret ----------
    function insertAtCaret(content, contentType) {
        var s = state();
        if (!s) { return { ok: false, error: 'editor state unavailable' }; }
        try {
            var v = view();
            var schema = s.schema;
            var tr = s.tr;
            var from = s.selection.from, to = s.selection.to;
            var caretM = caretMarks(s);

            if (contentType === 'PlainText') {
                // Insert at the caret while preserving block structure and
                // using exactly the marks that would apply to typed text
                // (stored marks preferred, else the caret's context marks):
                // first line goes inline into the parent block; following
                // lines continue as sibling blocks of the same type.
                var shared = sharedTextblockParent(s.doc, from, to);
                if (shared) {
                    tr = structuredTextInsert(tr, schema, shared.from, shared.parent, from, to, content, caretM);
                } else {
                    var blocks = plainTextToBlocks(content, schema, caretM);
                    if (!blocks.length) { return { ok: false, error: 'parsed content is empty' }; }
                    tr = insertBlocks(tr, schema, from, to, blocks);
                    ensureDocNonEmpty(tr, schema);
                }
                placeCaret(tr, tr.selection.to);
                v.dispatch(tr);
                bump();
                lastDocSize = tr.doc.content.size;
                freezeSelection(null);
                return { ok: true, revision: revision };
            }

            var blocks = parseHtmlFragment(content, schema);
            if (!blocks.length) { return { ok: false, error: 'parsed content is empty' }; }

            var slice = sliceFrom(blocks, schema);
            if (slice) {
                tr = tr.replaceSelection(slice);
            } else {
                // Handle multi-block insertion properly
                if (blocks.length > 0) {
                    var first = blocks[0];
                    tr = tr.replaceSelectionWith(first);
                    // Insert remaining blocks
                    for (var i = 1; i < blocks.length; i++) {
                        var pos = tr.selection.from;
                        tr = tr.insert(pos, blocks[i]);
                    }
                } else {
                    tr = tr.insertText(blocksToText(blocks));
                }
            }
            ensureDocNonEmpty(tr, schema);
            var addedTrailingParagraph = ensureTrailingParagraphAfterTable(tr, schema, blocks);
            placeCaret(
                tr,
                addedTrailingParagraph ? tr.doc.content.size - 1 : tr.selection.to,
                addedTrailingParagraph);

            v.dispatch(tr);
            bump();
            lastDocSize = tr.doc.content.size;
            freezeSelection(null);
            return { ok: true, revision: revision };
        } catch (err) {
            return { ok: false, error: String(err && err.message ? err.message : err) };
        }
    }

    // ---------- geometry ----------
    function domRectToObj(r, editorRect) {
        return {
            x: r.left - editorRect.left,
            y: r.top - editorRect.top,
            width: r.width,
            height: r.height
        };
    }

    function getSelectionBounds() {
        var v = view();
        if (!v || !v.dom) { return []; }
        var sel = window.getSelection();
        if (!sel || sel.rangeCount === 0) { return []; }
        var editorRect = v.dom.getBoundingClientRect();
        var rects = [];
        for (var i = 0; i < sel.rangeCount; i++) {
            var range = sel.getRangeAt(i);
            var clientRects = range.getClientRects();
            if (clientRects.length) {
                for (var c = 0; c < clientRects.length; c++) {
                    rects.push(domRectToObj(clientRects[c], editorRect));
                }
            } else if (range.collapsed) {
                var b = caretRectFromRange(range);
                if (b) { rects.push(domRectToObj(b, editorRect)); }
            }
        }
        return rects;
    }

    function caretRectFromRange(range) {
        try {
            var probe = range.cloneRange();
            probe.collapse(false);
            var rects = probe.getClientRects();
            if (rects.length) { return rects[0]; }
            var span = document.createElement('span');
            span.textContent = '\u200b';
            probe.insertNode(span);
            var r = span.getBoundingClientRect();
            span.parentNode.removeChild(span);
            return r;
        } catch (e) { return null; }
    }

    function getCaretBounds() {
        var v = view();
        if (!v || !v.dom) { return null; }
        var sel = window.getSelection();
        if (!sel || sel.rangeCount === 0) { return null; }
        var editorRect = v.dom.getBoundingClientRect();
        var range = sel.getRangeAt(0).cloneRange();
        range.collapse(false);
        var rect = null;
        var rects = range.getClientRects();
        if (rects && rects.length) { rect = rects[0]; }
        if (!rect) {
            var span = document.createElement('span');
            span.textContent = '\u200b';
            try {
                range.insertNode(span);
                rect = span.getBoundingClientRect();
                span.parentNode.removeChild(span);
            } catch (e2) { return null; }
        }
        if (!rect || (rect.width === 0 && rect.height === 0)) { return null; }
        return domRectToObj(rect, editorRect);
    }

    // ---------- selection change notification ----------
    var lastNotified = { from: -1, to: -1 };
    setInterval(function () {
        var s = state();
        if (!s) { return; }
        var sel = s.selection;
        if (sel && (sel.from !== lastNotified.from || sel.to !== lastNotified.to)) {
            lastNotified = { from: sel.from, to: sel.to };
            var text = '';
            try { text = s.doc.textBetween(sel.from, sel.to, '\n', ''); } catch (e) { }
            // Preserve any genuine non-collapsed model selection immediately.
            // The request layer applies its own input-size limit; truncating
            // cached ranges here would leave valid longer selections without
            // the positions needed to replace them.
            if (sel.from < sel.to && text && text.length > 0) {
                freezeSelection({
                    revision: revision,
                    from: sel.from,
                    to: sel.to,
                    selectedText: text,
                    selectedHtml: ''   // filled on demand by captureSelection
                });
            } else if (sel.from === sel.to && editorHasDomFocus()) {
                // A caret change made while the editor still owns DOM focus is
                // an intentional deselection, not a toolbar-induced blur.
                freezeSelection(null);
            }
            if (window.__customRteOnSelectionChanged) {
                try {
                    window.__customRteOnSelectionChanged(
                        text, sel.from, sel.to, sel.from < sel.to);
                } catch (e2) { }
            }
        }
    }, 300);

    // ProseMirror may update its model selection between polling ticks. Freeze
    // that range as soon as the browser selection changes so a host toolbar
    // taking focus cannot erase the only copy of the user's selection.
    document.addEventListener('selectionchange', function () {
        // Capture the browser's current range first, even when ProseMirror has
        // not yet observed the same selection.
        captureSelection();
    }, true);

    // ---------- public surface ----------
    window.__customRte = {
        captureSelection: captureSelection,
        captureDocument: captureDocument,
        getStoredSelection: getStoredSelection,
        replaceRange: replaceRange,
        replaceDocument: replaceDocument,
        insertAtCaret: insertAtCaret,
        getSelectionBounds: getSelectionBounds,
        getCaretBounds: getCaretBounds,
        getRevision: function () { return revision; },
        // Exposed for diagnostics from the host: lets the .NET side peek at
        // the last non-collapsed selection that the freeze-snapshot logic
        // is holding onto. Returns null when nothing is frozen.
        peekFrozenSelection: function () { return frozenSelection; }
    };
})();
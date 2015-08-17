
var e = {
    "Id": 17,
    "Guid": "{2151c7fa-db22-45b4-b139-db5b91e0b08e}",
    "Type": "Product",
    "_TitleField": "Name",
    "Att": [
        {
            k: "Name",
            vs: [{ v: "Cambucha" }]
        },
        { k: "Image", vs: [{ v: "company-logo.jpg" }] },
        {
            k: "Intro",
            vs: [
                { v: "Try this product", d: { "en-en": "rw", "fr-fr": "r", "it-it": "r" } },
                { v: "Versuchen Sie das jetzt", d: { "de-de": "rw", "de-ch": "r" } }
            ]
        },
        { k: "Longo", vs: [] }
    ]
};


// Note: the entity-reader is meant for admin-purposes. 
// It does not try to do fallback, because the admin-UI MUST know the real data
function enhanceEntity(entity) {
    var enhancer = this;

    // this will enhance a ValueSet with necessary methods
    enhancer.enhanceVs = function (vs) {
        vs.hasLanguage = function (language) { return this.d.hasOwnProperty(language); };
        vs.setLanguage = function (language, shareMode) { this.d[language] = shareMode; };
        vs.languageMode = function (language) { return (this.hasLanguage(language)) ? this.d[language] : ""; }
        return vs;
    };

    // this will enhance an attribute
    enhancer.enhanceAtt = function (att) {
        att.getVsWithLanguage = function (language) {
            // try to find it based on the language - it then has a property matching the language
            for (var v = 0; v < this.vs.length; v++)
                if (this.vs[v].hasLanguage(language))
                    return this.vs[v];

            // if we don't find it, we must report it back as such
            return null;
        };

        att.setLanguageToVs = function (valueSet, language, shareMode) {
            // check if it's already there, if yes, just ensure shareMode, then done
            if (valueSet.hasLanguage(language))
                return valueSet.setLanguage(language, shareMode);

            // otherwise find the language if it's anywhere else and remove that first; 
            // note that this might delete a value set, so we should only do it after checking if it wasn't already right
            this.removeLanguage(language);

            // now set it anew
            return valueSet.setLanguage(language, shareMode);
        };


        att.removeLanguage = function (language) {
            var vs = this.getVsWithLanguage(language);
            if (vs === null)
                return;
            delete vs.d[language];

            // check if the vs still has any properties left, if not, remove it entirely - unless it's the last one...
            if (vs.d.length == 0 && this.vs.length > 0)
                this.removeVs(attribute, vs);
        };

        att.removeVs = function (valueSet) {
            for (var v = 0; v < this.vs.length; v++)
                if (this.vs[v] === valueSet)
                    delete this.vs[v];
        };

        // todo: when adding VS - ensure the events are added too...
        att.addVs = function (value, language) {
            var dimensions = new {};
            dimensions[language] = "rw";
            var newVs = { "v": value, "d": dimensions };
            this.vs.push(enhancer.enhanceVs(newVs));
        };

        // Now go through the ValueSets and give them more commands
        for (var v = 0; v < att.vs.length; v++)
            enhancer.enhanceVs(att.vs[v]);

        return att;
    }

    // this will enhance an entity
    enhancer.enhanceEntity = function (ent) {


        ent.getTitle = function () {
            ent.getAttribute(e._TitleField);
        };

        ent.hasAttribute = function (attrName) {
            for (var c = 0; c < ent.Att.length; c++)
                if (ent.Att[c].k == attrName)
                    return true;
            return false;
        };

        ent.getAttribute = function (attrName) {
            for (var c = 0; c < ent.Att.length; c++)
                if (ent.Att[c].k == attrName)
                    return ent.Att[c];
            return null;
        };


        for (var attCount = 0; attCount < ent.Att.length; attCount++)
            enhancer.enhanceAtt(ent.Att[attCount]);

        return ent;
    }

    return enhancer.enhanceEntity(entity);
};

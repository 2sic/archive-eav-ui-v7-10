
var testMlE = {
    "Id": 17,
    "Guid": "{2151c7fa-db22-45b4-b139-db5b91e0b08e}",
    "Type": "Product",
    "TitleAttributeName": "Name",
    "Attributes": [
        {
            Key: "Name",
            Values: [{ Value: "Cambucha" }]
        },
        { Key: "Image", Values: [{ Value: "company-logo.jpg" }] },
        {
            Key: "Intro",
            Values: [
                { Value: "Try this product", Dimensions: { "en-en": true, "fr-fr": false, "it-it": false } },
                { Value: "Versuchen Sie das jetzt", Dimensions: { "de-de": true, "de-ch": false } }
            ]
        },
        { Key: "Longo", Values: [] } 
    ]
};


describe("EAV Multilanguage Content", function () {
    it("Exists", function () {
        expect(enhanceEntity).toBeDefined();
    });

    describe("Demo data", function () { 
        it("exits", function () {
            expect(testMlE).toBeDefined();
        });
        it("is 17 and a product", function () {
            expect(testMlE.Id).toBe(17);
            expect(testMlE.Type).toBe("Product");
        });
    });

    var ee = enhanceEntity(testMlE); 

    it("Properly returns enhanced entity", function () {
        expect(ee).toBeDefined();  
    });

    describe("Entity extended with enhancements", function () {
        describe("Init works - entities are enhanced", function () {
            it("intro was found and has 2 values", function () {
                var intro = ee.getAttribute("Intro");
                expect(intro).toBeDefined();
                expect(intro.Values.length).toBe(2);
            });
            
            describe("the value for de-ch", function () {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("intro had a value for CH", function () {
                    expect(vsCH).toBeDefined();
                });
                it("shouldn't work for caps like DE-ch", function () {
                    expect(intro.getVsWithLanguage("DE-ch")).toBeNull();
                });
                xit("the value has 3 dimensions", function () {
                    expect(vsCH.Dimensions.length).toBe(3); 
                });
            });

            describe("changes to languages - adding a language should work", function () {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("shouldn't be definet at first", function () {
                    expect(vsCH.Dimensions["en-uk"]).toBeUndefined(); 
                });
                it("ch should now map to en-uk as well", function () {
                    intro.setLanguageToVs(vsCH, "en-uk", true);
                    expect(vsCH.Dimensions["en-uk"]).toBeDefined();
                });
            });
            
            describe("moving a language to another value", function () {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                describe("should simply move en-en from the first to the second", function () {
                    it("should have en in the fr-set before...", function () {
                        expect(intro.getVsWithLanguage("fr-fr").Dimensions["en-en"]).toBeDefined();
                    })
                    it("...but not afterwards, when it's on de-ch", function () {
                        intro.setLanguageToVs(vsCH, "en-en", false);
                        expect(intro.getVsWithLanguage("fr-fr").Dimensions["en-en"]).toBeUndefined();
                        expect(intro.getVsWithLanguage("de-ch").Dimensions["en-en"]).toBeDefined();
                    })
                })
            });

            describe("removing a language should work", function () {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("should have been on de-ch at first", function () {
                    expect(intro.getVsWithLanguage("de-ch").Dimensions["en-en"]).toBeDefined();
                });
                it("should go away now and not be found any more", function () {
                    intro.removeLanguage("en-en");
                    expect(intro.getVsWithLanguage("de-ch").Dimensions["en-en"]).toBeUndefined();
                    expect(intro.getVsWithLanguage("en-en")).toBeNull();
                })
            });

            describe("adding a completely new language should work", function () {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("shouldn't have ru-ru at first, but should afterwards", function () {
                    expect(intro.getVsWithLanguage("ru-ru")).toBeNull();
                    intro.addVs("Broznsk", "ru-ru");
                    var vsRu = intro.getVsWithLanguage("ru-ru");
                    expect(vsRu).toBeDefined();
                    expect(vsRu.Value).toBe('Broznsk');
                })
            });

            describe("removing a language without other languages should remove the value", function () {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("should have it at first, then remove the entire value", function () {
                    expect(intro.getVsWithLanguage("ru-ru")).toBeDefined();
                    expect(intro.Values.length).toBe(3);
                    intro.removeLanguage("ru-ru");
                    expect(intro.Values.length).toBe(2);
                    expect(intro.getVsWithLanguage("ru-ru")).toBeNull();
                }) 
            })
        });


    })
});
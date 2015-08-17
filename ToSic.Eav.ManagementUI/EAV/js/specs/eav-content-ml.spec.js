describe("EAV Multilanguage Content", function () {
    it("Exists", function () {
        expect(enhanceEntity).toBeDefined();
        //expect(helloWorld()).toEqual("Hello world!");
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
        describe("Attributes are enhanced", function () {
            var intro = ee.getAttribute("Intro");
            it("intro was found", function () {
                expect(intro).toBeDefined();
            });
            it("intro has 2 values", function () {
                expect(intro.vs.length).toBe(2);
            });
            
            var vsCH = intro.getVsWithLanguage("de-ch");
            describe("the value for de-ch", function () {
                it("intro had a value for CH", function () {
                    expect(vsCH).toBeDefined();
                });
                it("shouldn't work for caps like DE-ch", function () {
                    expect(intro.getVsWithLanguage("DE-ch")).toBeNull();
                });
                xit("the value has 3 dimensions", function () {
                    expect(vsCH.d.length).toBe(3);
                });
            });

            describe("changes to languages - adding a language should work", function () {
                it("shouldn't be definet at first", function () {
                    expect(vsCH.d["en-uk"]).toBeUndefined(); 
                });
                it("ch should now map to en-uk as well", function () {
                    intro.setLanguageToVs(vsCH, "en-uk", "rw");
                    expect(vsCH.d["en-uk"]).toBeDefined();
                });
            });
            
            describe("moving a language to another value", function () {
                describe("should simply move en-en from the first to the second", function () {
                    it("should have en in the fr-set before...", function () {
                        expect(intro.getVsWithLanguage("fr-fr").d["en-en"]).toBeDefined();
                    })
                    it("...but not afterwards, when it's on de-ch", function () {
                        intro.setLanguageToVs(vsCH, "en-en", "r");
                        expect(intro.getVsWithLanguage("fr-fr").d["en-en"]).toBeUndefined();
                        expect(intro.getVsWithLanguage("de-ch").d["en-en"]).toBeDefined();
                    })
                })
            })
        });


    })
});
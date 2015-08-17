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
            var vCH = intro.getVsWithLanguage("de-ch");
            describe("the value for CH", function () {
                it("intro had a value for CH", function () {
                    expect(vCH).toBeDefined();
                });
                xit("the value has 3 dimensions", function () {
                    expect(vCH.d.length).toBe(3);
                });
            });

            describe
        });


    })
});

// Manually created test-object
var testMlE = {
    "Id": 17,
    "Guid": "{2151c7fa-db22-45b4-b139-db5b91e0b08e}",
    "Type": { Name: "Product", StaticName: "c95030592039" },
    "TitleAttributeName": "Name",
    "Attributes": [
        { Key: "Name", Values: [{ Value: "Cambucha" }] },
        { Key: "Image", Values: [{ Value: "company-logo.jpg" }] },
        { Key: "Intro", Values: [
                { Value: "Try this product", Dimensions: { "en-en": true, "fr-fr": false, "it-it": false } },
                { Value: "Versuchen Sie das jetzt", Dimensions: { "de-de": true, "de-ch": false } }
            ] },
        { Key: "Longo", Values: [] } 
    ]
};

var compareValues = {
    Id: 17
};

// Test-Object #6494 based on WebService 2015-08-18 16:45 --> /api/EAV/EavContent/GetOne?appId=1&contentType=Store&id=6494&format=multi-language
var wsEntity = {
    "Id": 6494,
    "Guid": "cc5a4abf-ccf0-4d48-8d4f-84bb6b07979e",
    "Type": { "Name": "Store", "StaticName": "c9666f4a-40a0-4956-baf1-209a26c29d55" },
    "TitleAttributeName": "Name",
    "Attributes": [
        { "Key": "Tags", "Values": [{ "Value": "f7cf33e7-6c0d-4cdf-9c7a-8e112c7f4dfe", "Dimensions": {} }] },
        { "Key": "Name", "Values": [{ "Value": "mettlers home", "Dimensions": { "en-us": false } }] },
        { "Key": "Address", "Values": [{ "Value": "r&auml;fiserhalde 34", "Dimensions": { "en-us": false } }] },
        { "Key": "Zip", "Values": [{ "Value": "9470", "Dimensions": { "en-us": false } }] },
        { "Key": "City", "Values": [{ "Value": "Buchs sg", "Dimensions": { "en-us": false } }] },
        { "Key": "Latitude", "Values": [{ "Value": "47.1483808", "Dimensions": { "en-us": false } }] },
        { "Key": "Longitude", "Values": [{ "Value": "9.478604099999984", "Dimensions": { "en-us": false } }] },
        { "Key": "Latitude2", "Values": [{ "Value": "47.1483808", "Dimensions": { "en-us": false } }] },
        { "Key": "Longitude2", "Values": [{ "Value": "9.478604099999984", "Dimensions": { "en-us": false } }] }
    ]
};

// Test object #280 --> /api/EAV/EavContent/GetOne?appId=1&contentType=Person ML&id=280&format=multi-language
var ws280 = {
    "Id": 280,
    "Guid": "a4cec726-8035-4b67-9702-ab80eae253bc",
    "Type": { "Name": "Person ML", "StaticName": "5277aaa7-aac0-4190-9ecb-48e8bfc6a741" },
    "TitleAttributeName": "LastName",
    "Attributes": [
        {
            "Key": "LastName",
            "Values": [
                { "Value": "Gemperle 17:51", "Dimensions": { "en-us": false } },
                { "Value": "Gemperle DE", "Dimensions": { "de-de": false } }
            ]
        }, {
            "Key": "FirstName",
            "Values": [
                { "Value": "Benjamin 17:51", "Dimensions": { "en-us": false } },
                { "Value": "Benjamin DE", "Dimensions": { "de-de": false } }
            ]
        }, {
            "Key": "Address",
            "Values": [
                { "Value": "Churerstrasse 35 17:51 EN", "Dimensions": { "en-us": false } },
                { "Value": "Churerstrasse 35 DE\r\n23.05.2014 10:19", "Dimensions": { "de-de": false } }
            ]
        }, {
            "Key": "ZIP",
            "Values": [
                { "Value": "9470 17:51", "Dimensions": { "en-us": false } },
                { "Value": "9470 DE", "Dimensions": { "de-de": false } }
            ]
        }, {
            "Key": "City",
            "Values": [
                { "Value": "Buchs 17:51", "Dimensions": { "en-us": false } }
            ]
        }, {
            "Key": "Manager",
            "Values": [
                { "Value": "33027abe-d3ed-4ca1-9b4c-aa1f08667624", "Dimensions": {} }
            ]
        }
    ]
};

var compareValues6494 = {
    Id: 6494
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
            expect(testMlE.Id).toBe(compareValues.Id);
            expect(testMlE.Type.Name).toBe("Product");
        });
    });

    var ee = enhanceEntity(testMlE); 

    it("Properly returns enhanced entity", function () {
        expect(ee).toBeDefined();  
    });

    describe("Entity extended with enhancements", function() {
        describe("Init works - entities are enhanced", function() {
            it("intro was found and has 2 values", function() {
                var intro = ee.getAttribute("Intro");
                expect(intro).toBeDefined();
                expect(intro.Values.length).toBe(2);
            });

            describe("the value for de-ch", function() {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("intro had a value for CH", function() {
                    expect(vsCH).toBeDefined();
                });
                it("shouldn't work for caps like DE-ch", function() {
                    expect(intro.getVsWithLanguage("DE-ch")).toBeNull();
                });
                xit("the value has 3 dimensions", function() {
                    expect(vsCH.Dimensions.length).toBe(3);
                });
            });

            describe("changes to languages - adding a language should work", function() {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("shouldn't be definet at first", function() {
                    expect(vsCH.Dimensions["en-uk"]).toBeUndefined();
                });
                it("ch should now map to en-uk as well", function() {
                    intro.setLanguageToVs(vsCH, "en-uk", true);
                    expect(vsCH.Dimensions["en-uk"]).toBeDefined();
                });
            });

            describe("moving a language to another value", function() {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                describe("should simply move en-en from the first to the second", function() {
                    it("should have en in the fr-set before...", function() {
                        expect(intro.getVsWithLanguage("fr-fr").Dimensions["en-en"]).toBeDefined();
                    });
                    it("...but not afterwards, when it's on de-ch", function() {
                        intro.setLanguageToVs(vsCH, "en-en", false);
                        expect(intro.getVsWithLanguage("fr-fr").Dimensions["en-en"]).toBeUndefined();
                        expect(intro.getVsWithLanguage("de-ch").Dimensions["en-en"]).toBeDefined();
                    });
                });
            });

            describe("removing a language should work", function() {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("should have been on de-ch at first", function() {
                    expect(intro.getVsWithLanguage("de-ch").Dimensions["en-en"]).toBeDefined();
                });
                it("should go away now and not be found any more", function() {
                    intro.removeLanguage("en-en");
                    expect(intro.getVsWithLanguage("de-ch").Dimensions["en-en"]).toBeUndefined();
                    expect(intro.getVsWithLanguage("en-en")).toBeNull();
                });
            });

            describe("adding a completely new language should work", function() {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("shouldn't have ru-ru at first, but should afterwards", function() {
                    expect(intro.getVsWithLanguage("ru-ru")).toBeNull();
                    intro.addVs("Broznsk", "ru-ru");
                    var vsRu = intro.getVsWithLanguage("ru-ru");
                    expect(vsRu).toBeDefined();
                    expect(vsRu.Value).toBe('Broznsk');
                });
            });

            describe("removing a language without other languages should remove the value", function() {
                var intro = ee.getAttribute("Intro");
                var vsCH = intro.getVsWithLanguage("de-ch");
                it("should have it at first, then remove the entire value", function() {
                    expect(intro.getVsWithLanguage("ru-ru")).toBeDefined();
                    expect(intro.Values.length).toBe(3);
                    intro.removeLanguage("ru-ru");
                    expect(intro.Values.length).toBe(2);
                    expect(intro.getVsWithLanguage("ru-ru")).toBeNull();
                });
            });
        });
    });
});
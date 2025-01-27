﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Model;
using NUnit.Framework;
using UnityEngine;

namespace Tests {
    public class GameObjectTests {
        private string testJsonsLocation = "Assets/Editor/TestJsons/";
        private string singleItemJson = "single-item.json";
        private string singleItemListJson = "single-item-with-list.json";
        private string itemSchemaV1 = "item-schema-v1.json";

        private string getFilepath(string filename) {
            return string.Concat(testJsonsLocation, filename);
        }

        // Serializes a single GameObjectEntry
        [Test]
        public void TestSerializeSingleItem() {
            using (StreamReader r = new StreamReader(getFilepath(singleItemJson))) {
                string json = r.ReadToEnd();

                SchemaItem entry = JsonUtility.FromJson<SchemaItem>(json);

                // Assert fields are correct
                Assert.That(entry.item_id, Is.EqualTo(1));
                Assert.That(entry.name, Is.EqualTo("wood"));
                Assert.That(entry.type, Is.EqualTo(SchemaItem.ItemType.PrimaryResource));
            }
        }

        // Serializes a single GameObjectEntry containing a recipe list
        [Test]
        public void TestSerializeSingleItemWithList() {
            using (StreamReader r = new StreamReader(getFilepath(singleItemListJson))) {
                string json = r.ReadToEnd();

                SchemaItem entry = JsonUtility.FromJson<SchemaItem>(json);

                // Assert fields are correct
                Assert.That(entry.item_id, Is.EqualTo(8));
                Assert.That(entry.name, Is.EqualTo("steel"));
                Assert.That(entry.type, Is.EqualTo(SchemaItem.ItemType.BlueprintCraftedComponent));
                
                // Asserts recipe entry is correct
                Assert.That(entry.recipe[0].item_id, Is.EqualTo(4));
                Assert.That(entry.recipe[0].quantity, Is.EqualTo(1));
            }
        }

        // Serializes the whole item schema using GameObjectsHandler
        [Test]
        public void TestInitialiseGameObjectsHandler() {
            SchemaManager goh = SchemaManager.FromFilepath(getFilepath(itemSchemaV1));     
            
            //Assert fields are correct
            Assert.That(goh.GameObjs.items.Count, Is.EqualTo(16));

            Assert.That(goh.GameObjs.items[0].item_id, Is.EqualTo(1));
            Assert.That(goh.GameObjs.items[0].name, Is.EqualTo("wood"));
            Assert.That(goh.GameObjs.items[0].type, Is.EqualTo(SchemaItem.ItemType.PrimaryResource));
            
            Assert.That(goh.GameObjs.items[15].item_id, Is.EqualTo(16));
            Assert.That(goh.GameObjs.items[15].name, Is.EqualTo("dune buggy"));
            Assert.That(goh.GameObjs.items[15].type, Is.EqualTo(SchemaItem.ItemType.Intangible));
            Assert.That(goh.GameObjs.items[15].blueprint[0].item_id, Is.EqualTo(9));
            Assert.That(goh.GameObjs.items[15].blueprint[0].quantity, Is.EqualTo(4));
            Assert.That(goh.GameObjs.items[15].blueprint[1].item_id, Is.EqualTo(10));
            Assert.That(goh.GameObjs.items[15].blueprint[1].quantity, Is.EqualTo(1));
        }

        // Asserts that a blueprint is retrieved without exception
        [Test]
        public void TestGetSingleElementBlueprintPass() {
            // Serialize schema
            SchemaManager goh = SchemaManager.FromFilepath(getFilepath(itemSchemaV1));

            // Create list of available objects
            List<RecipeElement> availables = new List<RecipeElement>();
            availables.Add(new RecipeElement(2, 8));

            try {
                Optional<SchemaItem> goe = goh.GetBlueprint(availables, 7);
                
                //Assert values are correct
                Assert.That(goe.IsPresent());
                Assert.That(goe.Get().name, Is.EqualTo("furnace"));
            } catch (InvalidDataException e) {
                // Exception thrown, failure case
                Assert.Fail(e.Message);
            }
        }

        // Asserts that correct exception is thrown for invalid blueprint retrieval
        [Test]
        public void TestGetSingleElementBlueprintFail() {
            // Serialize schema
            SchemaManager goh = SchemaManager.FromFilepath(getFilepath(itemSchemaV1));

            // Create list of available objects, with too few values
            List<RecipeElement> availables = new List<RecipeElement>();
            availables.Add(new RecipeElement(2, 6));
            
            Optional<SchemaItem> goe = goh.GetBlueprint(availables, 7);

            if (goe.IsPresent()) {
                // Failure case
                // GetBlueprint returns non-null object where null expected
                Assert.Fail();
            }
        }

        // Asserts that a recipe is correctly returned
        [Test]
        public void TestGetRecipePass() {
            // Serialize schema
            SchemaManager goh = SchemaManager.FromFilepath(getFilepath(itemSchemaV1));
            
            // Create valid list of available objects
            List<RecipeElement> availables = new List<RecipeElement>();
            availables.Add(new RecipeElement(4, 1));
            availables.Add(new RecipeElement(5, 1));

            // Obtain valid output
            Optional<MachineProduct> goe = goh.GetRecipe(availables, 7);
            
            // Asserts
            Assert.That(goe.IsPresent());
            Assert.That(goe.Get().item.name, Is.EqualTo("steel"));
        }

        // Asserts that a null value is correctly returned where no recipe is valid
        [Test]
        public void TestGetRecipeFail() {
            // Serialize schema
            SchemaManager goh = SchemaManager.FromFilepath(getFilepath(itemSchemaV1));
            
            // Create valid list of available objects
            List<RecipeElement> availables = new List<RecipeElement>();
            availables.Add(new RecipeElement(5, 1));

            // Obtain valid output
            Optional<MachineProduct> goe = goh.GetRecipe(availables, 7);
            
            // Asserts
            if (goe.IsPresent()) {
                // Failure case, no objects are valid. Hence goe should be null
                Assert.Fail();
            }
        }
    }
}
